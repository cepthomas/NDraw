# NDraw
- Translates text description of a 2D drawing into a picture.
- Rectangles, ellipses, and simple lines supported.
- Requires VS2022 and .NET8.
- No dependencies on third party components.


# Usage
- Open an NDraw file in both the application and your favorite text editor. When you edit/save the file it is
  automatically re-parsed and re-drawn.
- UI  
    - `mousewheel` scrolls up/down.
    - `shift+mousewheel` scrolls left/right.
    - `ctrl+mousewheel` zooms in/out.
    - `H` resets the display.
    - Show/hide layers, grid, ruler.


# The File Format
- NDraw files consist of text lines describing elements to be drawn. The parameters are a comma separated list of `name=value`.
- Lines are parsed once from top to bottom.
- `//` is used for comments.
- Refer to `Test\test1.nd` as an example.

## Types
These types are supported in the shape definitions.

Expression may contain N and T types, and +- operators only e.g. `my_val=some_location - 2.33 + some_offset`.


| Type | Description      | Options        |
| ---- | -----------      | -------        |
| D    | Identifier       |                |
| I    | Integer          |                |
| F    | Float            |                |
| S    | String           |                |
| C    | Color            | html known name    |
| N    | Numerical value  | float OR integer OR expression |
| T    | Text value       | quoted string OR float OR integer OR expression |
| P    | Point style      | a(rrow) OR t(ee) |
| H    | Hatch style      | ho OR ve OR fd OR bd OR lg OR dc |
| A    | Text alignment   | tl OR tc OR tr OR ml OR mc OR mr OR bl OR bc OR br |


## Page
Page section describes aspects of the page itself.

```
page=pg_1, un="feet", gr=10.0, sc=5
```

Field | Type | Def | Description
----  | ---- | --- | ----------
un    | text    |  ""  | Units name
gr    | number    |  1.0  | Grid spacing in virtual units
sc    | number    |  1.0  | Scale - pixels per virtual unit


## Globals
Globals are defaults for similar parameters omitted from shapes. The may be modified at any time.

```
$lt=4
$lc=salmon
```

Field | Type | Def | Description
----  | ---- | --- | ----------
$fc   |  C   |  N  | Fill color
$lc   |  C   |  N  | Line color
$lt   |  F   |  N  | Line thickness
$ta   |  A   |  N  | Text alignment
$ss   |  P   |  N  | Start point style
$es   |  P   |  N  | End point style
        Color _fc = Color.LightBlue;
        Color _lc = Color.Red;
        float _lt = 2.5f;
        ContentAlignment _ta = ContentAlignment.MiddleCenter;
        PointStyle _ss = PointStyle.None;
        PointStyle _es = PointStyle.None;


## User Values
User values can be defined at any time. The lhs is an id that can be referred to in subsequent operations, and the rhs is type N.

```
size_1_w=10
size_2_w=size_1_w - 2.33
```


## Shapes
All shapes have a common set of parameters.

Field | Type | Req | Description
----  | ---- | --- | ----------
lr    |  I   |  N  | Layer 1 to 4 or 0 for all
tx    |  T   |  N  | Display text
fc    |  C   |  N  | Fill color
ht    |  H   |  N  | Hatch type
lc    |  C   |  N  | Line color
lt    |  F   |  N  | Line thickness
ta    |  A   |  N  | Text alignment


## Rectangle
Render a rectangle.

```
rect=my_rect1, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w + 4.4, h=size_1_h, lc=green, fc=lightgreen, tx="Nice day", ta=tl
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
ellipse=my_circle, x=50, y=50, w=20, h=20, tx="I'm rrrround", ta=mc

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
line=my_line1, lr=2, sx=loc_1_x, sy=loc_1_y, ex=loc_2_x, ey=loc_3_y, lt=2, tx=loc_1_x+100, ta=tl, ss=a, es=t

```

Field | Type | Req | Description
----  | ---- | --- | ----------
sx    |  N   |  Y  | Start x
sy    |  N   |  Y  | Start y
ex    |  N   |  Y  | End x
ey    |  N   |  Y  | End y
ss    |  P   |  N  | Start point style
es    |  P   |  N  | End point style


# External Components

This application uses these FOSS components:
- Main icon: [Charlotte Schmidt](http://pattedemouche.free.fr/) (Copyright © 2009 of Charlotte Schmidt).
- Button icons: [Glyphicons Free](http://glyphicons.com/) (CC BY 3.0).
