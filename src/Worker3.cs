using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.TaskExample
{
  // Пример обработчика задачи с загрузкой ретроспективы
  public class Worker3 : BackgroundService
  {
    private const int WorkerDelay = 5000; // период задержки выполнения задачи, в мс

    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;


    public Worker3(ICommonInfrastructure infr,
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
      // загрузим ретроспективу ТИТ #TT20:4:1 с начала текущего часа
      var retro = await _api.GetAnalogRetro(new TmAnalog(20, 4, 1),
                                            new TmAnalogRetroFilter(GetDateTimeStartOfHour(), // с начала часа
                                                                    DateTime.Now, // до текущего момента
                                                                    null), // шаг определим автоматически, можно задать в мс
                                            0); // номер ретро определим аавтоматически, можно задать

      // создадим список значений ретроспективы (недостоверные значения отбраковываем)
      var retroValues = retro.Where(r => !r.IsUnreliable)
                             .Select(r => r.Value)
                             .ToList();
      
      // в ТИТ #TT20:4:2 запишем среднее значение по ретроспективе 
      await _api.SetAnalog(20, 4, 2, retroValues.Average());
      
      // в ТИТ #TT20:4:3 запишем максимальное значение по ретроспективе 
      await _api.SetAnalog(20, 4, 3, retroValues.Max());
    }


    private static DateTime GetDateTimeStartOfHour()
    {
      var now = DateTime.Now;
      return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, 0);
    }
  }
}