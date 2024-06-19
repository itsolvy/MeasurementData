## Установка окружения для локальной разработки

Для того чтобы начать работу с проектом достаточно установить SDK .NET 7. Инструкции по установке (Linux) и файлы установки (остальные ОС) можно найти тут: [https://dotnet.microsoft.com/en-us/download/dotnet/7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

Для запуска приложения нужно перейти в корневую папку (APRF-Backend) и выполнить команду:

```sh
dotnet watch --project=./APRF.Web/APRF.Web.csproj
```

Приложение запустится со стандартными настройками описанными в файлах appsettings.json и appsettings.\*.json (в зависимости от конфигурации)

## Настройка окружения для ОС Windows

Чтобы сделать разработку максимально удобной и быстрой потребуется установить дополнительные инструменты.

Общий список требуемых зависимостей и для чего они нужны:

- Git - Дистрибутив, идущий по умолчанию в составе Visual Studio урезанный
- Docker - требуется для запуска БД и тестов
- Powershellv7 – требуется для удобной работы с Makefile на Windows
- Dbmate – инструмент миграции БД
- Make – инструмент для работы с Makefile, который упрощает запуск рутинных команд
- LINQtoDBCLItools – набор инструментов командной строки для скаффолда БД
- PostgresCLItools – требуется для того чтобы генерировать schema.sql файл
- Husky.Net – инструмент для отслеживания git-хуков
- Csharpier – форматтер кода

Далее будут описаны шаги для установки каждой зависимости:

### Git

Установить последную версию 
[https://git-scm.com/downloads](https://git-scm.com/downloads)

### Docker

Файлы установки и инструкцию по работе с Docker можно найти тут:

[https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

### Powershell v7

Файлы установки и инструкции по работе с Powershellv7 можно найти тут:

[https://github.com/PowerShell/PowerShell/releases/tag/v7.3.3](https://github.com/PowerShell/PowerShell/releases/tag/v7.3.3)

### Dbmate

Описание процесса установки Dbmate можно найти тут:

[https://github.com/amacneil/dbmate](https://github.com/amacneil/dbmate)

В случае если лень читать, можно в Powershell выполнить следующие команды:

```sh
> Set-ExecutionPolicy RemoteSigned -Scope CurrentUser # Optional: Needed to run a remote script the first time
> irm get.scoop.sh | iex
> scoop install dbmate
```

### Make

Для установки make в Windows потребуется сначала установить choco (описано тут: [https://chocolatey.org/install](https://chocolatey.org/install))

После того как choco установлен – требуется выполнить следующую команду:

```sh
> choco install make
```

Или если лень читать, то запустите powershell под аднимистратором.
```sh
> Set-ExecutionPolicy Bypass -Scope Process -Force; 
> [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
> iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
> choco install make
```

### LINQ to DB CLI tools

Для установки LINQtoDBCLItools потребуется выполнить следующую команду:

```sh
 dotnet tool install -g linq2db.cli
```

### Postgres CLI tools

Для установки Postgres CLI tools потребуется скачать установщик PostgreSQL, при установке надо будет поставить галочку только на установку инструментов командной строки. Сам установщик можно найти тут:
[https://www.enterprisedb.com/downloads/postgres-postgresql-downloads](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads)

После установки инструментов командной строки потребуется указать путь до данной папки в переменную окружения Path. Как добавить значение в переменную среды Path – можно прочитать тут:
[https://learn.microsoft.com/ru-ru/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14)](<https://learn.microsoft.com/ru-ru/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14)>)

Добавить придется целиком путь до инструментов:
`C:\Program Files\PostgreSQL\15\bin`

Или иной путь, в зависимости от того, куда были установлены инструменты.

### Husky.Net

Husky.Net должен установиться автоматически при сборке проекта, но можно установить его и самостоятельно. Для установки нужно выполнить следующую команду:

```sh
> dotnet tool install --global Husky
```

### Csharpier

Для установки Csharpier потребуется запустить следующую команду:

```sh
> dotnet tool install csharpier -g
```

После установки можно также интегрировать инструмент в вашу IDE. Описание интеграции для популярных редакторов можно найти тут:
[https://csharpier.com/docs/Editors](https://csharpier.com/docs/Editors)

После того как все зависимости установлены – проверим работу инструментов. Сначала развернем локальную БД PostgreSQL в Docker, для этого выполним следующую команду:

```sh
> docker run -p 5432:5432 -e POSTGRES_PASSWORD=12345 -e POSTGRES_USER=postgres -d postgres:15.3
```

После того как БД будет запущена в контейнере – запустим миграции, для этого перейдем в корневую папку решения (`APRF-Backend`) и выполним команду:

```sh
> dbmate -d "./APRF.Web/DB/Migrations" -s "./APRF.Web/DB/schema.sql" -u "postgres://postgres:12345@localhost:5432/aprf?sslmode=disable" --no-dump-schema up
```

Либо для простого запуска миграций можем воспользоваться командой make, для этого также выполним команду в корневой папке решения (APRF-Backend):

```sh
> make db_up
```

Остальные команды make-файла и их предназначение будут описаны ниже.

Для того чтобы приложение использовало локальную БД и не изменять `appSettings.json`, потребуется добавить переменную окружения с названием `ConnectionStrings__Aprf` (**`ДВА ПОДЧЕРКИВАНИЯ!`**) и значением `Server=localhost;Port=5432;User ID=postgres;Password=12345;Database=aprf;Integrated Security=true;`

Как добавить переменную среды можно почитать [тут](https://learn.microsoft.com/ru-ru/sql/integration-services/lesson-1-1-creating-working-folders-and-environment-variables?view=sql-server-ver16)

Теперь, когда БД развернута, миграции успешно выполнились и переменная окружения добавлена – запустим приложение, для этого перейдем в корневую папку решения (`APRF-Backend`) и выполним команду:

```sh
> dotnet watch --project=./APRF.Web/APRF.Web.csproj
```

Либо для простого запуска приложения можем воспользоваться командой make, для этого также выполним команду в корневой папке решения (`APRF-Backend`):

```sh
> make run
```