﻿using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorComponent
{
    public abstract partial class BMain : BDomComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected CssBuilder WrapCssBuilder { get; } = new CssBuilder();
    }
}