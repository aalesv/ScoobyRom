# gnuplot TEMPLATE for Table2D

# Copyright (C) 2011-2015 SubaruDieselCrew
#
# This file is part of ScoobyRom.
#
# ScoobyRom is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# ScoobyRom is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with ScoobyRom.  If not, see <http://www.gnu.org/licenses/>.

# See template file for 3D surface plot, more documentation in there!

set macros
dataFile = ARG1
#print dataFile

# interactive terminals tested on Windows: qt, wxt, windows
# override terminal if needed, better use default (i.e. OS environment variable GNUTERM)
# set term qt

#show xlabel
#show ylabel
#show title
# variable set by app: windowtitle = "..."
#print windowtitle

# font and size
# Gnuplot-defined variable GPVAL_TERM has currently active terminal i.e. "qt"
set term GPVAL_TERM font "sans,12

set term GPVAL_TERM title windowtitle

set termoption enhanced

set grid

style_lines = "lines linetype -1 linecolor \"red\" linewidth 3"
style_points = "points linetype -1 linecolor \"blue\" pointtype 7 pointsize 1.0"

# lines and points have same color
#plot dataFile binary volatile title "" with linespoints linetype 1 pointtype 7 linewidth 2 linecolor "red"

# two combined plots to allow separate colors for both lines and points
plot dataFile binary volatile title "" with @style_points, dataFile binary volatile title "" with @style_lines
