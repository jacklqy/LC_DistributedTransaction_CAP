## 分布式的代价：
分布式环境下，服务器之间的通信，可能是不靠谱，这种情况无法避免分区容错，一定存在。CAP是不能同时满足的！

Consistency 和 Availability怎么选？一致性和可用性，不能同时满足，要什么？

CP重要，一致性最重要了，数据不能错，银行—交易数据

AP重要，可用性最重要了，系统的可用性，尤其是分布式----微服务，可用性尤为重要，没有可用性是跑不起来

## CAP

1、Consistency(一致性)：意思是写操作之后的读操作，必须返回该值，不能出现一个不正确的结果。

 a)强一致性：任意时刻数据都是一致性的。

 b)弱一致性性：允许某一时刻不一致，承诺在一定时间内变成一致。

 c)最终一致性：允许数据不一致，但是最终最终，数据还是的一致的。

2、Availability(可用性)：意思是只要收到用户的请求，服务器就必须给出回应。（分布式下面可用性是最重要的）

3、Partition tolerance(分区容错)：大多数分布式系统都分布在多个子网络。每个子网络就叫做一个区（partition）。分区容错的意思是，区间通信可能失败。比如，一台服务器放在中国，另一台服务器放在美国，这就是两个区，它们之间可能无法通信。


## 多种一致性：

1、强一致性---任意时刻数据都是一致的

2PC(预备-》就绪-》提交)：阻塞导致性能问题、 单点故障(事务管理器挂了)、消息丢失问题。=》DTC是基于数据库层面的事务 .NET Framework 下MSDTC实现，Distributed Transaction Coordinator服务必须开启（演示的是单机---局域网需要配置）。 .NET Core不支持—Linux

3PC(预备-》就绪-》预提交-》提交)：阻塞导致性能问题、 单点故障(事务管理器挂了)、消息丢失问题。只是增加了数据库自动提交。

2、弱一致性---允许某一时刻不一致，承诺在一定时间内变成一致的--Try-Confirm-Cancel。=》主要用在银行、阿里---钱必须保障-不能阻塞—设计负责，开发工作重ByteTCC、Himly、TCC-transaction事务管理器—这些都是Java的 .NET没有—所以解决方案也没有—除非自己写

3、最终一致性---允许数据不一致，但是最终最终，

数据还是得一致的 业务层面


## 幂等性
对同一个系统，使用同样的条件，一次请求和重复的多次请求对系统资源的影响是一致的。(网页提交按钮，点1次 跟10次结果一样)。

场景：支付—页面修改信息---订单减库存

1 MVCC多版本并发控制—乐观锁---数据库更新时带上版本号---更新+1 条件必带version-----id + version

2 去重表---请求带个guid---操作前校验下guid---点赞—100赞-不能重复—文章id+用户id+唯一索引

3 Token机制---每次操作都带个唯一id，请求来了先检测再执行

## 本地消息表分布式事务
MQ分布式事务--本地消息表--基于消息的一致性



