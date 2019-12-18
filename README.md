# EastLog

简单通用的日志记录和查询工具

> 功能  

基于 `Serilog` 扩展出一套日志组件, 规范日志格式. 实现日志的写入和查询.  
日志的格式

```C#
/*
app        : 程序标识
category1  : 分类1 (默认方法所在的文件夹名称)
category2  : 分类2 (默认方法所在的.cs文件名)
category3  : 分类3 (默认方法名)
log        : 日志内容(可以是一段字符串或者一个对象(对象将会以json的形式存入))
filter1    : 过滤1 用于精确定位日志 (比如可以是当前登录人员的id)
filter2    : 过滤2 (过滤1的扩展字段)
ip         : 服务器的地址(不太准确)
trace      : 追踪Id, 同一次http请求生产多条日志, 那么这些日志的此值将会相同
calls      : 方法调用链(a调用b,b调用c,最终在c中生产日志, 那么将会记录["a","b","c"])
exception  : 异常
*/
$"{app},{category1},{category2},{category3},{log},{filter1},{filter2},{ip},{trace},{calls},{exception}"
```

> 基础功能  (开发中)

[√] 写入日志  
[×] 读取日志  
[×] 日志追踪

## 启用日志

在 `Startup.cs` 的 `ConfigureServices` 方法中注入 `EasyLogger` .

```C#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddHttpContextAccessor(); // 注入HttpContextAccessor, 此项设置是trace值的关键, 如果丢失这个设置trace恒为null
    ...
    services.AddEasyLogger();
    ...
}
```

同文件的 `Configure` 方法中设置 `EasyLogger`

```C#
    ...
    app.UseEasyLogger(options =>
    {
        options.App = "SampleWeb"; // 指定app名称,当有个app同时使用日志组件时将会使用这个字段区分它们
        options.Serilog  // 需要赋值一个 Serilog.Core.Logger 实例 通常如下所示
            = new LoggerConfiguration()
                .WriteTo.ColoredConsole( // 输出日志到控制台
                    restrictedToMinimumLevel: LogEventLevel.Debug // 一些设置...
                )
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9222/")) // 输出日志到 es
                {
                    AutoRegisterTemplate = true,
                    MinimumLogEventLevel = LogEventLevel.Debug
                })
                .CreateLogger();
    });
    ...
```

生产日志

```C#
private readonly EasyLogger _log;

public Class1(EasyLogger log) // 注入EasyLogger实例
{
    _log = log;
}

public void TestLog()
{
    _log.Information("just test"); // 记录一个字符串

    _log.Information(new { mission = "just test" }); // 记录一个object
}
```

最终记录的日志内容

```text
easy_logger("SampleWeb","Services","Class1","TestLog()","just test",null,null,"10.0.75.1","1a013666-7eb7-49ce-99e3-2ebb3932c304",["SampleWeb.Controllers.WeatherForecastController.Get()", "SampleWeb.Services.LogTestService.Log()"],null)

easy_logger("SampleWeb","Services","Class1","TestLog()","{\"mission\":\"just test\"}",null,null,"10.0.75.1","1a013666-7eb7-49ce-99e3-2ebb3932c304",["SampleWeb.Controllers.WeatherForecastController.Get()", "SampleWeb.Services.LogTestService.Log()"],null)
```

虽然生产日志的地方只传入了一个字符串, 但是根据当前日志的堆栈 `System.Environment.StackTrace` , 对日志添加 `{app},{category1},{category2},{category3},{calls}` 参数.   

根据 `HttpContextAccessor` 对日志添加 `{trace}` 参数.  
这就是最终日志的效果
