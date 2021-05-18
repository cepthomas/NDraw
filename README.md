# NDraw
- Translates text description of a 2D drawing into a picture.
- Rectangles, ellipses, and simple lines supported.
- Targets .NET 5 and C# 9. No dependencies on third party components.


# Usage
- Open the NDraw file (*.nd) in both the application and your favorite text editor. When you edit/save the file it is
  automatically re-parsed and re-drawn.
- UI  
    - `mousewheel` scrolls up/down.
    - `shift+mousewheel` scrolls left/right.
    - `ctrl+mousewheel` zooms in/out.
    - `H` resets the display.


# The File Format
- NDraw files consist of single lines describing elements to be drawn. The parameters are a comma separated list of `name=value`.
- Lines are parsed once from top to bottom.
- `//` is used for comments.
- Refer to `test1.nd` in the Test directory for example.

## Types
These types are supported in the shape definitions.

Expression may contain N and T types, and +- operators only e.g. `my_val=some_location - 2.33 + some_offset`.


| Type | Description
| ---- | ------
| D    | Identifier
| I    | Integer
| F    | Float
| S    | String
| C    | Color name - html known
| N    | Numerical value: float \| integer \| expression
| T    | Text value: quoted string \| float \| integer \| expression
| P    | Point style: n(one) \| a(rrow) \| t(ee)
| A    | Text alignment: tl \| tc \| tr \| ml \| mc \| mr \| bl \| bc \| br 


## Page
Page section describes aspects of the page itself.

```
pg_1=page, un="feet", gr=10.0, sc=5
```

Field | Type | Req | Description
----  | ---- | --- | ----------
un    | T    |  N  | Units name
gr    | F    |  Y  | Grid spacing in virtual units
sc    | S    |  Y  | Pixels per virtual unit


## Globals
Globals are defaults for similar parameters omitted from shapes. The may be modified at any time.

```
$lt=4
$lc=salmon
```

Field | Type | Req | Description
----  | ---- | --- | ----------
$fc   |  C   |  N  | Fill color
$lc   |  C   |  N  | Line color
$lt   |  F   |  N  | Line thickness
$tp   |  A   |  N  | Text alignment
$ss   |  P   |  N  | Start point style
$es   |  P   |  N  | End point style


## User Values
User values can be defined at any time. The lhs is an id that can be referred to in subsequent operations, and the rhs is type N.

```
size_1_w=10
size_2_w=size_1_w - 2.33
```


## Shapes
All shapes have a common set of parameters.

```
xxxx
```

Field | Type | Req | Description
----  | ---- | --- | ----------
id    |  D   |  N  | Identifier
lr    |  I   |  N  | 1 to N or 0 for all
tx    |  T   |  N  | Display text
fc    |  C   |  N  | Fill color
lc    |  C   |  N  | Line color
lt    |  F   |  N  | Line thickness
tp    |  A   |  N  | Text alignment


## Rectangle
Render a rectangle.

```
my_rect1=rect, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w + 4.4, h=size_1_h, lc=green, fc=lightgreen, tx="Nice day", tp=tl
```

Field | Type | Req | Description
----  | ---- | --- | ----------
x     |  N   |  Y  | Top left x
y     |  N   |  Y  | Top left y
w     |  N   |  Y  | Width
h     |  N   |  Y  | Height


## Ellipse
Render an ellipse.

```
my_circle=ellipse, x=50, y=50, w=20, h=20, tx="I'm rrrround", tp=mc

```

Field | Type | Req | Description
----  | ---- | --- | ----------
x     |  N   |  Y  | Center x
y     |  N   |  Y  | Center y
w     |  N   |  Y  | Width
h     |  N   |  Y  | Height


## Line
Render a line.

```
my_line1=line, lr=2, sx=loc_1_x, sy=loc_1_y, ex=loc_2_x, ey=loc_3_y, lt=2, tx=loc_1_x+100, tp=tl, ss=a, es=t

```

Field | Type | Req | Description
----  | ---- | --- | ----------
sx    |  N   |  Y  | Start x
sy    |  N   |  Y  | Start y
ex    |  N   |  Y  | End x
ey    |  N   |  Y  | End y
ss    |  N   |  N  | Start point style
es    |  N   |  N  | End point style
