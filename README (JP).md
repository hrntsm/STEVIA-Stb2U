# STEVIA
![](./Assets/Textures/banner.jpg)

STEVIA は 構造架構の可視化ツールです。  
ST-Bridge file(.stb) を読み込んで、表示をします。  
部材の色分け表示機能などがあり、架構のチェックやプレゼンテーションを容易にすることを目的としています。

## What is ST-Bridge

ST-Bridgeの規格を作成している [building SMART Japan の構造設計小委員会 様](https://www.building-smart.or.jp/meeting/buildall/structural-design/) での記載を引用します。

> ST-Bridgeとは・・・日本国内の建築構造分野での情報交換のための標準フォーマット
> + 利用範囲を明確にすることによって、IFCよりシンプルで扱い易い
> + 日本独自の表現方法を取り込む（通り芯、部材配置と断面符号、配筋情報）
> + 国内の構造系アプリ、躯体積算アプリ、3次元オブジェクトCADとの連携を目指す

## Supported Features

+ 対象は ST-Bridge 1.4 (ver2 は非対応)
+ 部材表示
  + 柱、間柱、大梁、小梁、ブレース、スラブ
+ 配筋表示
  + 柱、間柱、大梁、小梁
+ チェックボックスによる部材・配筋の表示/非表示の切り替え
+ 部材の表示色の変更
  + RGBA で0～1の間での入力に対応
  + 配筋についてはデフォルト色のみで変更不可
  + CFT,SRCは色変更に対応していないためマゼンタで表示
+ カメラのの焦点距離の設定
  + デフォルト値は50mm 

## Environment

+ Unity 2018.4.13f1 で開発
+ Windows10 で動作確認

## Lisence

[MIT](./LICENSE)

## Contact information

詳細のお問い合わせ、バグ等については以下よりご連絡ください。

+ Twitter : [@hiron_rgkr](https://twitter.com/hiron_rgkr)
+ URL : [https://rgkr-memo.blogspot.com/](https://rgkr-memo.blogspot.com/)
+ E-mail : stevia(at)hrntsm.com 
  + change (at) to @