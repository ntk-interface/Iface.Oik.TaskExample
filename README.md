## Iface.Oik.TaskExample

Программа для разработчиков с примерами вызова API для связи с сервером «ОИК Диспетчер НТ».

Для использования, написания и отладки программы рекомендуется использовать Visual Studio 2022 с установленным .NET SDK 6 или выше.

Для отладки из среды разработки нужно указать следующие параметры запуска:

    сервер_динамических_данных компьютер имя_пользователя пароль
    
Например:

    TMS 10.0.0.69 admin password
	
При дальнейшем использовании в качестве внешней задачи сервера, аргументы указывать не требуется.

Для работы программы на рабочем сервере должен быть установлен .NET Desktop Runtime 6.x - https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## Кратко о принципе работы кода

### Program.cs
Точка входа в программу. Внутри вызывается функция связи с сервером ОИК Диспетчер НТ, регистрируются требуемые службы.

Файл не требует редактирования, кроме указания классов, содержащих логику задачи, которые напишет разработчик.

Для каждого класса требуется добавить следующую строку с указанием его названия (в примере Worker):

```
services.AddHostedService<Worker>();
```

### TmStartup.cs
Описание установки связи с сервером ОИК Диспетчер НТ и разрыв соединения в случае получения соответствующей команды от сервера.

Файл не требует редактирования, кроме указания имени программы:

```
private const string ApplicationName = "TaskExample";
private const string TraceName       = "TaskExample";
private const string TraceComment    = "<Iface.Oik.TaskExample>";
```

### Worker.cs (Worker2.cs и т.д.)
Этот и последущий класс в примерах реализует требуемую логику задачи - чтение данных с сервера, выполнение каких-то расчетов, запись результатов, подача команд и т.д.

Все они для простоты работают по общему принципу: при старте задачи вызывается метод ExecuteAsync, внутри которого в бесконечном цикле (до получения команды остановки программы) будет вызываться основной метод DoWork, с паузой между вызовами, заданной в константе WorkerDelay.

При необходимости можно полностью изменить принцип работы, это просто пример с минимальными усилиями по добавлению собственных расчетов.

Можно как пользоваться всего одним классом, так и создать сколько угодно классов логики, каждый из которых будет работать в собственном потоке, независимо от остальных.

Главное, чтобы каждый из этих классов был описан в файле Program.cs (см. выше).