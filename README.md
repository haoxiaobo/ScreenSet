# Screen Set 快速设置屏幕

## 说明

如果你有两个以上的显示器，而且经常要切换显示器的纵横组合、排列方式，那么本程序可以帮助你快速完成切换。

## 使用方法：

1. 运行本程序。可以设置为开机自动运行，自动隐藏窗口什么的。
2. 用“保存当前模式”的按钮，把现在的屏幕排列方案保存起来。
3. 你可以组合出不同的方案，然后都执行一次保存模式。
4. 现在，可以用系统热键来完成自己常用模式的切换了。
   1. Ctrl + Alt + Win + `  :  （Esc下面的那个键）循环切换你保存下来的模式。
   2. Ctrl + Alt + Win + 1-5 : 切换到你保存的模式中对应的编号。
   3. Ctrl + Alt + Win + 0 : 一键重置到全部横屏，依次排开。
   4. Ctrl + Alt + Win + Up: 叫出主窗口。

## 系统需求

- 一个以上显示器，不然就没什么用了。
- windows 7 或更高。
- .Net framework 4.7.2 （如果你需要适配低版本的.net, 请告诉我。
- 一个有windows键的键盘。

## 关于技术

- 使用EnumDisplayDevices来取得所有的显示器信息。
- 使用ChangeDisplaySettingsEx来设置显示器模式。
  - 这里有个坑：当一个位置上已经有显示器时，再设置其它显示器，系统会自动调整位置以防止重叠。
  - 解决办法是：每次调整时，把第一个显示器设置为0，0，然后其它的显示器的位置都重新计算相对位置。
- 索取源码请到 https://github.com/haoxiaobo/ScreenSet
