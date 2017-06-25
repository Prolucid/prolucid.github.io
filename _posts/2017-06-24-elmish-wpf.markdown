---
layout: post
title:  "Elmish.WPF - Elm Architecture for WPF Applications"
date:   2017-06-24 19:08:21 -0400
categories: F# WPF XAML MVVM
---

Today we're releasing the first beta of Elmish.WPF, which provides an Elm-like architecture for writing WPF applications.

The library essentially replaces the traditional way of writing WPF apps, using MVVM or code-behind, with the simple yet reliable mode-update-view architecture used in Elm applications. Instead of worrying about notifying your view that a property changed, or raising a `CanExecuteChanged()` event, simply provide a state, a way to update that state, and the XAML that you want to display with typical Bindings.

Please [check it out](https://github.com/Prolucid/Elmish.WPF) and let us know what you think!
