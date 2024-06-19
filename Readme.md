## ��������� ��������� ��� ��������� ����������

��� ���� ����� ������ ������ � �������� ���������� ���������� SDK .NET 7. ���������� �� ��������� (Linux) � ����� ��������� (��������� ��) ����� ����� ���: [https://dotnet.microsoft.com/en-us/download/dotnet/7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

��� ������� ���������� ����� ������� � �������� ����� (APRF-Backend) � ��������� �������:

```sh
dotnet watch --project=./APRF.Web/APRF.Web.csproj
```

���������� ���������� �� ������������ ����������� ���������� � ������ appsettings.json � appsettings.\*.json (� ����������� �� ������������)

## ��������� ��������� ��� �� Windows

����� ������� ���������� ����������� ������� � ������� ����������� ���������� �������������� �����������.

����� ������ ��������� ������������ � ��� ���� ��� �����:

- Git - �����������, ������ �� ��������� � ������� Visual Studio ���������
- Docker - ��������� ��� ������� �� � ������
- Powershellv7 � ��������� ��� ������� ������ � Makefile �� Windows
- Dbmate � ���������� �������� ��
- Make � ���������� ��� ������ � Makefile, ������� �������� ������ �������� ������
- LINQtoDBCLItools � ����� ������������ ��������� ������ ��� ��������� ��
- PostgresCLItools � ��������� ��� ���� ����� ������������ schema.sql ����
- Husky.Net � ���������� ��� ������������ git-�����
- Csharpier � ��������� ����

����� ����� ������� ���� ��� ��������� ������ �����������:

### Git

���������� ��������� ������ 
[https://git-scm.com/downloads](https://git-scm.com/downloads)

### Docker

����� ��������� � ���������� �� ������ � Docker ����� ����� ���:

[https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)

### Powershell v7

����� ��������� � ���������� �� ������ � Powershellv7 ����� ����� ���:

[https://github.com/PowerShell/PowerShell/releases/tag/v7.3.3](https://github.com/PowerShell/PowerShell/releases/tag/v7.3.3)

### Dbmate

�������� �������� ��������� Dbmate ����� ����� ���:

[https://github.com/amacneil/dbmate](https://github.com/amacneil/dbmate)

� ������ ���� ���� ������, ����� � Powershell ��������� ��������� �������:

```sh
> Set-ExecutionPolicy RemoteSigned -Scope CurrentUser # Optional: Needed to run a remote script the first time
> irm get.scoop.sh | iex
> scoop install dbmate
```

### Make

��� ��������� make � Windows ����������� ������� ���������� choco (������� ���: [https://chocolatey.org/install](https://chocolatey.org/install))

����� ���� ��� choco ���������� � ��������� ��������� ��������� �������:

```sh
> choco install make
```

��� ���� ���� ������, �� ��������� powershell ��� ���������������.
```sh
> Set-ExecutionPolicy Bypass -Scope Process -Force; 
> [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
> iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
> choco install make
```

### LINQ to DB CLI tools

��� ��������� LINQtoDBCLItools ����������� ��������� ��������� �������:

```sh
 dotnet tool install -g linq2db.cli
```

### Postgres CLI tools

��� ��������� Postgres CLI tools ����������� ������� ���������� PostgreSQL, ��� ��������� ���� ����� ��������� ������� ������ �� ��������� ������������ ��������� ������. ��� ���������� ����� ����� ���:
[https://www.enterprisedb.com/downloads/postgres-postgresql-downloads](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads)

����� ��������� ������������ ��������� ������ ����������� ������� ���� �� ������ ����� � ���������� ��������� Path. ��� �������� �������� � ���������� ����� Path � ����� ��������� ���:
[https://learn.microsoft.com/ru-ru/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14)](<https://learn.microsoft.com/ru-ru/previous-versions/office/developer/sharepoint-2010/ee537574(v=office.14)>)

�������� �������� ������� ���� �� ������������:
`C:\Program Files\PostgreSQL\15\bin`

��� ���� ����, � ����������� �� ����, ���� ���� ����������� �����������.

### Husky.Net

Husky.Net ������ ������������ ������������� ��� ������ �������, �� ����� ���������� ��� � ��������������. ��� ��������� ����� ��������� ��������� �������:

```sh
> dotnet tool install --global Husky
```

### Csharpier

��� ��������� Csharpier ����������� ��������� ��������� �������:

```sh
> dotnet tool install csharpier -g
```

����� ��������� ����� ����� ������������� ���������� � ���� IDE. �������� ���������� ��� ���������� ���������� ����� ����� ���:
[https://csharpier.com/docs/Editors](https://csharpier.com/docs/Editors)

����� ���� ��� ��� ����������� ����������� � �������� ������ ������������. ������� ��������� ��������� �� PostgreSQL � Docker, ��� ����� �������� ��������� �������:

```sh
> docker run -p 5432:5432 -e POSTGRES_PASSWORD=12345 -e POSTGRES_USER=postgres -d postgres:15.3
```

����� ���� ��� �� ����� �������� � ���������� � �������� ��������, ��� ����� �������� � �������� ����� ������� (`APRF-Backend`) � �������� �������:

```sh
> dbmate -d "./APRF.Web/DB/Migrations" -s "./APRF.Web/DB/schema.sql" -u "postgres://postgres:12345@localhost:5432/aprf?sslmode=disable" --no-dump-schema up
```

���� ��� �������� ������� �������� ����� ��������������� �������� make, ��� ����� ����� �������� ������� � �������� ����� ������� (APRF-Backend):

```sh
> make db_up
```

��������� ������� make-����� � �� �������������� ����� ������� ����.

��� ���� ����� ���������� ������������ ��������� �� � �� �������� `appSettings.json`, ����������� �������� ���������� ��������� � ��������� `ConnectionStrings__Aprf` (**`��� �������������!`**) � ��������� `Server=localhost;Port=5432;User ID=postgres;Password=12345;Database=aprf;Integrated Security=true;`

��� �������� ���������� ����� ����� �������� [���](https://learn.microsoft.com/ru-ru/sql/integration-services/lesson-1-1-creating-working-folders-and-environment-variables?view=sql-server-ver16)

������, ����� �� ����������, �������� ������� ����������� � ���������� ��������� ��������� � �������� ����������, ��� ����� �������� � �������� ����� ������� (`APRF-Backend`) � �������� �������:

```sh
> dotnet watch --project=./APRF.Web/APRF.Web.csproj
```

���� ��� �������� ������� ���������� ����� ��������������� �������� make, ��� ����� ����� �������� ������� � �������� ����� ������� (`APRF-Backend`):

```sh
> make run
```