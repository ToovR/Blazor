// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Performance
{
    public class RenderTreeDiffBuilderBenchmark
    {
        private readonly Renderer renderer;
        private readonly RenderTreeBuilder original;
        private readonly RenderTreeBuilder modified;

        public RenderTreeDiffBuilderBenchmark()
        {
            renderer = new FakeRenderer();

            // A simple component for basic tests -- this is similar to what MVC scaffolding generates
            // for bootstrap3 form fields, but modified to be more Blazorey.
            original = new RenderTreeBuilder(renderer);
            original.OpenComponent<SimpleComponent>(0);
            original.OpenElement(1, "div");
            original.AddAttribute(2, "class", "form-group");

            original.OpenElement(3, "label");
            original.AddAttribute(4, "class", "control-label");
            original.AddAttribute(5, "for", "name");
            original.AddAttribute(6, "data-unvalidated", true);
            original.AddContent(7, "Car");
            original.CloseElement();

            original.OpenElement(8, "input");
            original.AddAttribute(9, "class", "form-control");
            original.AddAttribute(10, "type", "text");
            original.AddAttribute(11, "name", "name"); // Notice the gap in sequence numbers
            original.AddAttribute(13, "value", "");
            original.CloseElement();

            original.OpenElement(14, "span");
            original.AddAttribute(15, "class", "text-danger field-validation-valid");
            original.AddContent(16, "");
            original.CloseElement();

            original.CloseElement();
            original.CloseComponent();

            // Now simulate some input
            modified = new RenderTreeBuilder(renderer);
            modified.OpenComponent<SimpleComponent>(0);
            modified.OpenElement(1, "div");
            modified.AddAttribute(2, "class", "form-group");

            modified.OpenElement(3, "label");
            modified.AddAttribute(4, "class", "control-label");
            modified.AddAttribute(5, "for", "name");
            modified.AddAttribute(6, "data-unvalidated", false);
            modified.AddContent(7, "Car");
            modified.CloseElement();
            
            modified.OpenElement(8, "input");
            modified.AddAttribute(9, "class", "form-control");
            modified.AddAttribute(10, "type", "text");
            modified.AddAttribute(11, "name", "name");
            modified.AddAttribute(12, "data-validation-state", "invalid");
            modified.AddAttribute(13, "value", "Lamborghini");
            modified.CloseElement();
            
            modified.OpenElement(14, "span");
            modified.AddAttribute(15, "class", "text-danger field-validation-invalid"); // changed
            modified.AddContent(16, "No, you can't afford that.");
            modified.CloseElement();
            
            modified.CloseElement();
            modified.CloseComponent();
        }

        [Benchmark(Description = "RenderTreeDiffBuilder: Input and validation on a single form field.")]
        public void ComputeDiff_SingleFormField()
        {
            GC.KeepAlive(ComputeDiff(original, modified));
        }

        private RenderTreeDiff ComputeDiff(RenderTreeBuilder from, RenderTreeBuilder to)
        {
            var batchBuilder = new RenderBatchBuilder();
            return RenderTreeDiffBuilder.ComputeDiff(renderer, batchBuilder, 0, from.GetFrames(), to.GetFrames());
        }

        private class SimpleComponent : IComponent
        {
            public void Init(RenderHandle renderHandle)
            {
            }

            public void SetParameters(ParameterCollection parameters)
            {
            }
        }

        private class FakeRenderer : Renderer
        {
            public FakeRenderer()
                : base(new TestServiceProvider())
            {
            }

            protected override void UpdateDisplay(RenderBatch renderBatch)
            {
            }
        }

        private class TestServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return null;
            }
        }
    }
}
