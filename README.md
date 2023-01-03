# Buddhabrot

[![macOS Build & Test](https://github.com/davidaramant/buddhabrot/actions/workflows/macos-build.yml/badge.svg)](https://github.com/davidaramant/buddhabrot/actions/workflows/macos-build.yml)
[![Windows Build & Test](https://github.com/davidaramant/buddhabrot/actions/workflows/windows-build.yml/badge.svg)](https://github.com/davidaramant/buddhabrot/actions/workflows/windows-build.yml)

Tools to create a massive Buddhabrot rendering.  Written in C#/.NET 7.

WIP. The eventual result will be a collection of programs that can be used to create a rendering that can be viewed in a web browser.

The suite is composed of the following programs:

## Boundary Finder

Finds the border regions of the Mandelbrot set where the high-iteration Buddhabrot points live.

## Boundary Explorer

A GUI program to visually explore the boundary data sets.

## Point Finder

Churns through the border regions and finds interesting points to plot.

## Plot Generator

Plots the points to an intermediate datafile. 

## Renderer

Renders image tiles out of the plot.

