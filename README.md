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

| Type    | Description      | Options                               |
| ----    | -----------      | -------                               |
| number  | numerical value  | float, integer, expression            |
| text    | text value       | quoted string, unquoted string        |
| point   | float x/y pair   |                                       |
| color   | color            | html known name                       |
| enum    | point style      | a(rrow), t(ee), n(one)                |
| enum    | hatch style      | ho, ve, fd, bd, lg, dc                |
| enum    | dash style       | sld, dsh, dot, dd, ddd                |
| enum    | text alignment   | tl, tc, tr, ml, mc, mr, bl, bc, br    |

Expression may contain numbers, var names, +, - e.g. `my_val=some_location - 2.33 + some_offset`.

In the following tables `Def` is the default value for the field or `REQ` which indicates it is required.

## Page
Page section describes aspects of the page itself.

```
page=pg_1, un="feet", gr=10.0, sc=5
```

Field | Type      | Def       | Description
----  | ----      | ---       | ----------
un    | text      | ""        | units name
gr    | number    | 1.0       | grid spacing in virtual units
sc    | number    | 1.0       | scale - pixels per virtual unit


## Globals
Globals are defaults for similar parameters omitted from shapes. The may be modified at any time.

```
$lt=4
$lc=salmon
```

Field | Type      | Def          | Description
----  | ----      | ---          | ----------
$lr   |  text     |              | layer name or empty for all
$fc   |  color    | ghostwhite   | fill color
$lc   |  color    | dimgray      | line color
$lt   |  number   | 2.0          | line thickness
$ld   |  enum     | sld          | line dash
$ta   |  enum     | mc           | text alignment
$ss   |  enum     | n            | start point style
$es   |  enum     | n            | end point style


## User Values
User values can be defined at any time. The lhs is an id that can be referred to in subsequent operations, and the rhs is type N.

```
size_1_w=10
size_2_w=size_1_w - 2.33
```

## Shapes
All shapes have a common set of parameters.

Field | Type      | Def     | Description
----  | ----      | ---     | ----------
lr    |  number   | 1       | layer 1 to 4 or 0 for all
tx    |  text     | ""      | display text
fc    |  color    | $fc     | fill color
ht    |  enum     | n       | hatch type
lc    |  color    | $lc     | line color
lt    |  number   | $lt     | line thickness
ld    |  enum     | sld     | line dash
ta    |  enum     | $ta     | text alignment


## Rectangle
Render a rectangle.

```
rect=my_rect1, lr=1, x=loc_2_x, y=loc_2_y, w=size_1_w + 4.4, h=size_1_h, lc=green, fc=lightgreen, tx="Nice day", ta=tl
```

Field | Type      | Def   | Description
----  | ----      | ---   | ----------
x     |  number   | REQ   | top left x
y     |  number   | REQ   | top left y
w     |  number   | REQ   | width
h     |  number   | REQ   | height


## Ellipse
Render an ellipse.

```
ellipse=my_circle, x=50, y=50, w=20, h=20, tx="I'm rrrround", ta=mc

```

Field | Type      | Def   | Description
----  | ----      | ---   | ----------
x     |  number   | REQ   | center x
y     |  number   | REQ   | center y
w     |  number   | REQ   | width
h     |  number   | REQ   | height


## Line
Render a line.

```
line=my_line1, lr=2, sx=loc_1_x, sy=loc_1_y, ex=loc_2_x, ey=loc_3_y, lt=2, tx=loc_1_x+100, ta=tl, ss=a, es=t

```

Field | Type      | Def   | Description
----  | ----      | ---   | ----------
sx    |  number   |  REQ  | start x
sy    |  number   |  REQ  | start y
ex    |  number   |  REQ  | end x
ey    |  number   |  REQ  | end y
ss    |  point    |  n    | start point style
es    |  point    |  n    | end point style


# External Components

This application uses these FOSS components:
- Main icon: [Charlotte Schmidt](http://pattedemouche.free.fr/) (Copyright © 2009 of Charlotte Schmidt).
- Button icons: [Glyphicons Free](http://glyphicons.com/) (CC BY 3.0).
