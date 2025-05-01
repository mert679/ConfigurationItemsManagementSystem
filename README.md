# Kütüphanenin Amacı
Bu kütüphane, merkezi bir konfigürasyon yönetim sistemi sağlamayı amaçlar. Mikroservis mimarilerinde veya birden fazla uygulamanın ortak yapılandırma değerlerine ihtiyaç duyduğu senaryolarda, dinamik ve güncellenebilir ayarları merkezi bir veritabanından okuma imkânı sunar. Böylece her uygulama kendi ayarlarını kod içine gömmek yerine merkezi olarak yönetilen bir yapıdan çekebilir.



# Kütüphane Kodlarının Görevi
Bu kütüphane için nuget package den indirilmesi gereken araçlar:
1. Dapper:  Hafif ve hızlı bir ORM (Object Relational Mapper) kütüphanesidir. SQL sorgularını sade bir şekilde yazıp C# nesnelerine map eder.
2. Microsoft.EntityFrameworkCore: EFC object-database mapper for .Net.
3. Npgsql.EntityFrameworkCore.PostgreSQL: PostgreSQL veritabanına bağlanmak için kullanılan .NET uyumlu ADO.NET sağlayıcısıdır.


## ConfigurationReader
Uygulama ayağa kalktığında veritabanına bağlanarak ilgili uygulamaya ait aktif konfigürasyonları çeker ve bunları bellekte cache’ler. Belirli aralıklarla bu cache otomatik olarak güncellenir.
### Alanlar(Fields)
```
    private readonly string _appName;
    private readonly string _connectionString;
    private readonly System.Timers.Timer _timer;
    private readonly Dictionary<string, ConfigurationItems> _cache = new();
    private readonly int _refreshIntervalMs;

```
_appName: Hangi uygulamaya ait ayarların yükleneceğini belirtir.
_connectionString: PostgreSQL veritabanı bağlantı bilgilerini içeririr.
_timer: Belirli aralıklarla ayarların güncellenmesini sağlayan zamanlayıcı
_cache: Ayarların bellekte tutulduğu dictionary yapısı
_refreshIntervalMs: Ayarların ne sıklıkla güncelleneceğini belirten zaman arağılı(milisaniye cinsinden)
### Constructor
```
public ConfigurationReader(string applicationName, string connectionString, int refreshIntervalMs)

```
Gerekli bilgileri alır (appName, connectionString, refreshIntervalMs).
Hemen konfigürasyonları yükler: LoadConfigurations()
Belirtilen süre ile çalışan bir timer başlatır; bu timer her sürede bir LoadConfigurations() metodunu çağırarak konfigürasyonları günceller.
### Timer Mantığı
```
_timer.Elapsed += (sender,args) => LoadConfigurations();
```
Timer, her refreshIntervalMs süresinde bir tetkilenir ve ayarlar tekrar yüklenir. Böylece veritabanında bir değişiklik olduğunda, uygulama kısa süre sonra fark eder.

### LoadConfigurations()
Bu metodun görevi, veritabanına bağlanarak aktif ve uygulamaya özel konfigürasyonları okumak ve _cache içine kaydetmektir.
1. Veritabanı bağlantısı açılır.
2. SELECT sorgusu hazırlanır.
```
SELECT * FROM "ConfigurationItems" WHERE "IsActive" = TRUE AND "ApplicationName" = @App
```
3. Dapper ile sorgu çalıştırılır ve ConfigurationItems map edilir
4. _cache dictionary temizlenir ve yeni değerlerle güncellenir.


### GetValue<T>()
Bu method görevi, key parametresi ile cache'den değer alır ve alınan değeri istenilen T tipine çevirip döner.
```
    public T GetValue<T>(string key)
```

1. _cache içinde key kontrol edilir.
2. Varsa: Convert.ChangeType(...) ile string olan veritabanı değeri istenilen türe çevrilir.
Örneğin: int, bool, double, DateTime, vs.
3. Yoksa:KeyNotFoundException fırlatılır.
Örnek Kulanımı
```
    var isEnabled = configReader.GetValue<bool>("FeatureXEnabled");
```


## ConfigurationItems
Veritabanındaki ayarları temsil eden bir modeldir. Her bir konfigürasyon değeri Name,Type, Value, ApplicationName, IsActive gibi alanları içerir.
```

    public class ConfigurationItems
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
        public string ApplicationName { get; set; }
    }

```


## Library Herhangi bir projeye nasıl entegre edebiliriz?
1. Bu kütüphanemizi kullandığımız herhangi bir app entegre edebilmek için Bağımlılıklar(Dependency) sağ tıklayarak Proje başvurusu ekle tıklayıp derlediğimiz
kütüphaneyi projemize entegre edebiriz.
2. Uygulamanızın başlangıcında ConfigurationReader örneğini oluşturun:
```
var configReader = new ConfigurationReader("SERVICE-A", connectionString, 60000);
```
3. Konfigürasyon değeri almak için:
```
var siteName = configReader.GetValue<string>("SiteName");

```
### Sisteme Nasıl Entegre Edilir?
1. Veritabanına bağlantı sağlayan bir connection string oluşturun.
2. ConfigurationItems tablosunun şemasını veritabanınıza ekleyin.
3. Kütüphaneyi DI container’a veya uygun bir yapıya entegre ederek servis olarak kullanın.
4. Uygulamanın ihtiyaç duyduğu tüm konfigürasyonları bu yapı üzerinden yönetin.


## Web App'in Görevi
Web uygulaması (Web App), konfigürasyon değerlerinin görsel arayüz üzerinden yönetilmesini sağlar. Yöneticiler veya ilgili kişiler, belirli bir uygulamaya ait konfigürasyonları burada düzenleyebilir, aktif/pasif hale getirebilir ya da yeni ayar ekleyebilir. Böylece teknik olmayan kullanıcılar da ayarlara müdahale edebilir.
### Kullanılan Yapılar ve Pattern'ler
1. Singleton Pattern (isteğe bağlı): ConfigurationReader tek bir örnek üzerinden çalışacak şekilde yapılandırılabilir.
2. Repository Pattern: Veritabanı işlemleri  doğrudan repository-like mantıkla yapılır.
3. Timer + Polling: Belirli aralıklarla veritabanı sorgusu çalıştırılır.
4. Caching: Ayarlar bellekte tutulur ve performans artışı sağlanır.
5. Dependency Injection (kullanım şekline göre): DI ile ConfigurationReader sistem geneline enjekte edilebilir.

## .Net Core MVC projesine Kütüphaneyi nasıl entegre ettim?
### Program.cs 
Program.cs dosyasında Kütüphanemi tanımladım ve bilgilerini verdim artık projeye çalışır olduğu müdetçe kütüphane çalışmış olucak.
```
builder.Services.AddSingleton<ConfigurationReader>(provider =>
    new ConfigurationReader("SERVICE-A",
        builder.Configuration.GetConnectionString("DefaultConnection"),
        60000));
```

### HomeController
Test için bazı config key lerini HomeController da tanımlayıp site içerisinde gösterdim.

```
 private readonly ConfigurationReader _configReader;

 public HomeController(ConfigurationReader confReader)
 {
     _configReader = confReader;
 }

 public IActionResult Index()
 {
     try
     {
         string myValue = _configReader.GetValue<string>("IsBasketEnabled");
         ViewBag.ConfigValue = myValue;
     }
     catch (Exception ex)
     {
         ViewBag.ConfigValue = $"Hata: {ex.Message}";
     }

     return View();
 }
```

## Projeyi build etmeden önce kontrol edilmesi gerekenler
Lütfen  PostgreSQL kulanının ve orda proje için database oluşturun ve migration işlemlerini yapınız. Hem kütüphane hem de MVC projesinde
PostgreSQL kulanılmıştır.

```
dotnet ef migrations add InitialCreate
dotnet ef database update

```
