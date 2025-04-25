using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.TaskExample
{
  // Пример обработчика задачи, делает разное с сигналами и измерениями
  public class Worker : BackgroundService
  {
    private const int WorkerDelay = 1000; // период задержки выполнения задачи, в мс

    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;


    public Worker(ICommonInfrastructure infr,
                  IOikDataApi           api)
    {
      _infr = infr;
      _api  = api;
    }


    // вызывается один раз после старта
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested) // пока не получим сообщение о закрытии программы
      {
        await DoWork();
        await Task.Delay(WorkerDelay, stoppingToken);
      }
    }


    // собственно обработка действий задачи
    public async Task DoWork()
    {
      // выводим в трассировку имя пользователя
      Tms.PrintDebug(_infr.TmUserInfo?.Name);

      // выводим в трассировку текущее время сервера
      Tms.PrintDebug(await _api.GetSystemTimeString());

      // работа с сигналом
      // простой вариант
      var tsValue = await _api.GetStatus(20, 1, 1); // получаем значение #TC20:1:1
      await _api.SetStatus(20, 1, 2, tsValue);      // запишем его же значение в соседний #TC20:1:2

      // углубленный вариант
      var ts = new TmStatus(20, 1, 1);                // создаем #TC20:1:1
      await _api.UpdateTagPropertiesAndClassData(ts); // загружаем с сервера сведения о ТС (наименование, класс и т.п.)
      await _api.UpdateStatus(ts);                    // загружаем с сервера текущее состояния ТС и флаги
      Tms.PrintDebug(ts);                             // выводим в трассировку наименование и состояние

      // работа с измерением
      // простой вариант
      var tiValue = await _api.GetAnalog(20, 1, 1);   // получаем значение #TT20:1:1
      await _api.SetAnalog(20, 1, 2, tiValue);        // запишем его же значение в соседний #TT20:1:2
      
      // углубленный вариант
      var ti = new TmAnalog(20, 1, 1);                // создаем #TT20:1:1
      await _api.UpdateTagPropertiesAndClassData(ti); // загружаем с сервера сведения о ТИ (наименование, ед. изм. и т.п.)
      await _api.UpdateAnalog(ti);                    // загружаем с сервера текущее значение ТИ и флаги
      Tms.PrintDebug(ti);                             // выводим в трассировку наименование и значение
      
      // в цикле выставим всем сигналам из списка #TC20:1:1..10 признак недостоверности
      for (var i = 1; i <= 10; i++)
      {
        await _api.SetTagFlagsExplicitly(new TmStatus(20, 1, i), TmFlags.Unreliable);
      }
      // в цикле убираем всем измерениям из списка #TT20:1:1..5 признак установки вручную
      for (var i = 1; i <= 5; i++)
      {
        await _api.ClearTagFlagsExplicitly(new TmAnalog(20, 1, i), TmFlags.ManuallySet);
      }
      
      // подача команды телеуправления на #TC20:1:1
      var controlStatus = new TmStatus(20, 1, 1); // создаем объект сигнала
      var controlResult = await _api.Telecontrol(controlStatus); // подаем команду переключения (без указания нового состояния)
      Tms.PrintDebug($"Результат ТУ на {controlStatus} -> {controlResult}"); // выводим на экран результат команды
      
      // подача команды телеуправления ВКЛ на #TC20:1:2
      var controlStatus2 = new TmStatus(20, 1, 2); // создаем объект сигнала
      var controlResult2 = await _api.TelecontrolExplicitly(controlStatus2, 1); // подаем команду переключения ВКЛ
      Tms.PrintDebug($"Результат ТУ на {controlStatus2} -> {controlResult2}"); // выводим на экран результат команды
    }
  }
}