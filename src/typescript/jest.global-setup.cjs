/**
 * Jest global setup: pre-bundle chevrotain (a pure-ESM package) to CommonJS
 * so that Jest's CJS test runner can require() it normally.
 */
"use strict";

const esbuild = require("esbuild");
const path = require("path");
const fs = require("fs");

const OUTPUT_DIR = path.join(__dirname, ".jest-cache");
const OUTPUT_FILE = path.join(OUTPUT_DIR, "chevrotain-cjs.js");

async function setup() {
  if (!fs.existsSync(OUTPUT_DIR)) {
    fs.mkdirSync(OUTPUT_DIR, { recursive: true });
  }

  await esbuild.build({
    entryPoints: [require.resolve("chevrotain")],
    bundle: true,
    format: "cjs",
    platform: "node",
    outfile: OUTPUT_FILE,
    logLevel: "silent",
  });
}

module.exports = setup;
