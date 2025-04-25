using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Helpers;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.TaskExample
{
  // Пример обработчика задачи с накопленим текущего значения измерения
  public class Worker4 : BackgroundService
  {
    private const int WorkerDelay = 500; // период задержки выполнения задачи, в мс

    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;


    private          float       _sum    = 0;
    private readonly List<float> _values = new(); // список для хранения значений измерения


    public Worker4(ICommonInfrastructure infr,
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
      // загружаем текущее значение
      var value = await _api.GetAnalog(20, 1, 1);

      // добавляем его в список значений
      _values.Add(value);

      // храним только последние 60 значений, если размер превышает, то удаляем самое старое
      if (_values.Count > 60)
      {
        _values.RemoveAt(0);
      }

      // выведем на экран количество накопленных значений, сумму и среднее
      Tms.PrintDebug($"Всего: {_values.Count}, сумма: {_values.Sum()}, среднее: {_values.Average()}");

      // считаем сумму всех значений с самого начала
      _sum += value;

      // выведем на экран
      Tms.PrintDebug($"Общая сумма: {_sum}");
    }
  }
}