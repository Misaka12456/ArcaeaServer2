# Arcaea Server 2

高并发低占用的 Arcaea API 后端 基于ASP.NET Core 6.0


##### 支持的Arcaea客户端版本

* Arcaea 3.10.0(c) 及以上版本


##### 项目结构

* [Team123it.Arcaea.MarveCube](./Team123it.Arcaea.MarveCube) - Arcaea Server 2 主服务器后端

* [Team123it.Arcaea.MarveCube.Standalone](./Team123it.Arcaea.MarveCube.Standalone) - Arcaea Server 2 独立下载服务器后端

##### 运行环境（主服务器程序与下载服务器均需要)

* Microsoft.AspNetCore.App x64 运行时 6.0.0 及以上版本


##### 额外依赖环境（仅主服务器程序需要)

* MySQL 8.0+ / MariaDB 10.0+ (用于存储服务器数据)

* Redis 6.0+ (Windows端为Redis for Windows 3.0+) (用于存放下载Token等临时数据)


##### 特点

* 对于曲包id为 `unranked` 或难度定数为0(不存在应为-1)的曲目，程序会将成绩存储至 `bests_special` 而并非 `bests` 表中，因此这些曲目将不计入Best30计算

* 玩家的个人游玩潜力值计算中仅存在Best30，不存在Recent10，因此潜力值在任何情况下都不会出现倒扣的情况

* 玩家在曲目游玩结束并提交成绩后会视成绩以及难度给予玩家一定数量的记忆源点


##### 搭建之前……

独立的下载服务器(Team123it.Arcaea.MarveCube.Standalone)可以与主服务器程序放在一起；
但我们极力建议您将其放在与主服务器不同的、带宽较为充足的服务器上，以减轻主服务器的带宽负担，并增强主服务器的安全性。

主服务器和下载服务器都需要存在谱面文件夹(包括该曲目的ogg音频文件以及aff谱面文件)(位置在 `{程序根目录}\data\static\Songs` )，其作用如下：

1. 下载服务器为玩家提供数据下载

2. 主服务器在玩家提交数据后检查MD5校验值是否正确

3. 主服务器在玩家登录账号后返回所有谱面以及音频文件的MD5校验值


##### 运行之前……

1. 启动数据库程序 & Redis程序

2. 启动主服务器程序并按照提示进行初始化


##### 注意事项

* 为减轻文件读写压力，Arcaea Server 2 服务端在第一次收到登录请求时，会将谱面文件夹中的所有ogg音频文件以及aff谱面文件的MD5校验值，保存在数据库的 `fixed_songs_checksum` 表中。

* 若后续出现再次登录将直接返回数据库中存储的校验值而并非重新遍历计算校验值。

* 但使用该方法时可能会出现谱面文件/音频文件需要更新的情况，这时请手动删除 `fixed_songs_checksum` 表中的对应文件的MD5校验值项，下一位玩家登录后将会自动更新MD5校验值。
  
  
##### 关于Link Play

* 当前暂时不支持Link Play游玩，还请等待后续更新。


##### 开源协议

本企划基于[123 Open-Source Organization MIT Public License 2.0](https://team123it.github.io/LICENSE.html)许可协议开源。