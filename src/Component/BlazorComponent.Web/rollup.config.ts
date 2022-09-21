import { defineConfig } from "rollup";
import { terser } from "rollup-plugin-terser";

import typescript from "@rollup/plugin-typescript";

export default defineConfig({
  input: "./src/main.ts",
  output: [
    {
      file: "../BlazorComponent/wwwroot/js/blazor-component.js",
      format: "esm",
      sourcemap: true,
    },
  ],

  plugins: [typescript(), terser({
    keep_fnames: true
  })],
  watch: {
    exclude: "node_modules/**",
  },
});