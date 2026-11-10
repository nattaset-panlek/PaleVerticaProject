#!/usr/bin/env node
// Turns a high-resolution shape (alpha of a PNG, for example a Figma export at scale 4) into a
// signed distance field PNG that a shader can draw sharp at any size.
// Usage: node Tools/SdfFromPng/make_sdf.js <input.png> <output.png> [outputWidth=512] [spreadSourcePx=24]
// Output: grey PNG, 0.5 = edge, brighter = inside, darker = outside. Same aspect as the input.

const path = require('path');
const { decode, encode } = require(path.join(__dirname, 'png.js'));

const [input, output, widthArg, spreadArg] = process.argv.slice(2);
if (!input || !output) {
  console.error('Usage: node make_sdf.js <input.png> <output.png> [outputWidth=512] [spreadSourcePx=24]');
  process.exit(1);
}
const outWidth = Number(widthArg || 512);
const spread = Number(spreadArg || 24);
const INF = 1e20;

function edt1d(f, n, d, v, z) {
  let k = 0;
  v[0] = 0;
  z[0] = -INF;
  z[1] = INF;
  for (let q = 1; q < n; q++) {
    let s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2 * q - 2 * v[k]);
    while (s <= z[k]) {
      k--;
      s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2 * q - 2 * v[k]);
    }
    k++;
    v[k] = q;
    z[k] = s;
    z[k + 1] = INF;
  }
  k = 0;
  for (let q = 0; q < n; q++) {
    while (z[k + 1] < q) k++;
    d[q] = (q - v[k]) * (q - v[k]) + f[v[k]];
  }
}

function edt2d(grid, w, h) {
  const n = Math.max(w, h);
  const f = new Float64Array(n), d = new Float64Array(n), v = new Int32Array(n), z = new Float64Array(n + 1);
  for (let x = 0; x < w; x++) {
    for (let y = 0; y < h; y++) f[y] = grid[y * w + x];
    edt1d(f, h, d, v, z);
    for (let y = 0; y < h; y++) grid[y * w + x] = d[y];
  }
  for (let y = 0; y < h; y++) {
    for (let x = 0; x < w; x++) f[x] = grid[y * w + x];
    edt1d(f, w, d, v, z);
    for (let x = 0; x < w; x++) grid[y * w + x] = d[x];
  }
  return grid;
}

const image = decode(input);
const { w, h, px } = image;
const outside = new Float64Array(w * h);
const inside = new Float64Array(w * h);
for (let i = 0; i < w * h; i++) {
  const solid = px[i * 4 + 3] >= 128;
  outside[i] = solid ? 0 : INF;
  inside[i] = solid ? INF : 0;
}
edt2d(outside, w, h);
edt2d(inside, w, h);

const outHeight = Math.round(outWidth * h / w);
const scaleX = w / outWidth;
const scaleY = h / outHeight;
const result = Buffer.alloc(outWidth * outHeight * 4);
for (let oy = 0; oy < outHeight; oy++) {
  for (let ox = 0; ox < outWidth; ox++) {
    let sum = 0, samples = 0;
    for (let sy = 0; sy < 2; sy++) {
      for (let sx = 0; sx < 2; sx++) {
        const x = Math.min(w - 1, Math.floor((ox + 0.25 + sx * 0.5) * scaleX));
        const y = Math.min(h - 1, Math.floor((oy + 0.25 + sy * 0.5) * scaleY));
        const i = y * w + x;
        sum += Math.sqrt(outside[i]) - Math.sqrt(inside[i]);
        samples++;
      }
    }
    const distance = sum / samples;
    const value = Math.max(0, Math.min(255, Math.round((0.5 - distance / (2 * spread)) * 255)));
    const o = (oy * outWidth + ox) * 4;
    result[o] = result[o + 1] = result[o + 2] = value;
    result[o + 3] = 255;
  }
}
encode(output, outWidth, outHeight, result);
console.log(`Wrote ${output}: ${outWidth}x${outHeight} from ${w}x${h}, spread ${spread} source px (${(spread / scaleX).toFixed(1)} output px)`);
