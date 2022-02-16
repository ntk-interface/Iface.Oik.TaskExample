using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Iface.Oik.TaskExample
{
  // Пример обработчика задачи, по списку обновляет  
  public class Worker2 : BackgroundService
  {
    private const int WorkerDelay = 1000; // период задержки выполнения задачи, в мс

    private readonly ICommonInfrastructure _infr;
    private readonly IOikDataApi           _api;

    // активные мощности
    private readonly List<TmAnalog> _activePowers = new()
    {
      new TmAnalog(20, 1, 1),
      new TmAnalog(20, 4, 1)
    };

    // реактивные
    private readonly List<TmAnalog> _reactivePowers = new()
    {
      new TmAnalog(20, 1, 2),
      new TmAnalog(20, 4, 2),
    };

    // полные
    private readonly List<TmAnalog> _apparentPowers = new()
    {
      new TmAnalog(20, 1, 6),
      new TmAnalog(20, 4, 3),
    };


    public Worker2(ICommonInfrastructure infr,
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
      // обновим текущие значения активных и реактивных мощностей
      await _api.UpdateAnalogs(_activePowers);
      await _api.UpdateAnalogs(_reactivePowers);

      // в цикле посчитаем значения полных мощностей
      for (var i = 0; i < _activePowers.Count; i++)
      {
        var active   = _activePowers[i].Value;
        var reactive = _reactivePowers[i].Value;
        var apparent = Math.Sqrt(Math.Pow(active, 2) + Math.Pow(reactive, 2)); // считаем по обычной формуле

        var (ch, rtu, point) = _apparentPowers[i].TmAddr.GetTupleShort(); // получаем адрес канал, кп, объект
        await _api.SetAnalog(ch, rtu, point, (float)apparent);            // записываем значение
      }
    }
  }
}