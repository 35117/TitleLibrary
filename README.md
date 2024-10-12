# TitleLibrary
一个为称号系统设计的tshock插件，暂不支持其他插件和变量输入
可以保存称号让玩家自由切换
## 指令
### 给予称号
权限:TitleLibrary.GiveTitle  
/givetitle <name> <prepre/pre/suf/sufsuf> <title>或/gt  
给予玩家前前缀/前缀/后缀/后后缀并保存到配置中
### 切换称号
权限:TitleLibrary.ChangeTitle  
/changetitle <prepre/pre/suf/sufsuf> <num/list>或/ct  
来应用自己的前前缀/前缀/后缀/后后缀，如果参数是0则清空，如果参数是list则查看拥有的前前缀/前缀/后缀/后后缀
## 占位符
占位符在前前缀/前缀/后缀/后后缀中使用，用%%包括内容，替换为对应的内容
### Health
当前生命
### MaxHealth
最大生命
### MaxHealth2
最大生命2，500血喝了生命力，1显示500，2显示600
### Mana
当前魔力
### MaxMana
最大魔力
### MaxMana2
最大魔力2，区别暂不清楚
### HandItem
玩家手持物品名称
### Defense
玩家防御力
### Index
玩家索引
