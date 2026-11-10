const fs=require('fs'),zlib=require('zlib');
function decode(file){const b=fs.readFileSync(file);let p=8,w,h,ct,idat=[];while(p<b.length){const len=b.readUInt32BE(p),type=b.toString('ascii',p+4,p+8),d=b.slice(p+8,p+8+len);if(type==='IHDR'){w=d.readUInt32BE(0);h=d.readUInt32BE(4);ct=d[9];if(d[8]!==8)throw'bitdepth';}if(type==='IDAT')idat.push(d);p+=12+len;}
const bpp={6:4,2:3,0:1,4:2}[ct];const raw=zlib.inflateSync(Buffer.concat(idat));const px=Buffer.alloc(w*h*4);let prev=Buffer.alloc(w*bpp);
for(let y=0;y<h;y++){const f=raw[y*(w*bpp+1)];const line=Buffer.from(raw.slice(y*(w*bpp+1)+1,(y+1)*(w*bpp+1)));for(let i=0;i<line.length;i++){const a=i>=bpp?line[i-bpp]:0,up=prev[i],c=i>=bpp?prev[i-bpp]:0;let v=line[i];if(f===1)v+=a;else if(f===2)v+=up;else if(f===3)v+=(a+up)>>1;else if(f===4){const pp=a+up-c,pa=Math.abs(pp-a),pb=Math.abs(pp-up),pc=Math.abs(pp-c);v+=(pa<=pb&&pa<=pc)?a:(pb<=pc?up:c);}line[i]=v&255;}
for(let x=0;x<w;x++){const o=(y*w+x)*4;if(bpp===4){line.copy(px,o,x*4,x*4+4);}else if(bpp===3){px[o]=line[x*3];px[o+1]=line[x*3+1];px[o+2]=line[x*3+2];px[o+3]=255;}else{px[o]=px[o+1]=px[o+2]=line[x*bpp];px[o+3]=bpp===2?line[x*2+1]:255;}}prev=line;}
return{w,h,px};}
function crc(buf){let c=~0;for(const x of buf){c^=x;for(let k=0;k<8;k++)c=(c>>>1)^(0xEDB88320&-(c&1));}return ~c>>>0;}
function chunk(t,d){const l=Buffer.alloc(4);l.writeUInt32BE(d.length);const td=Buffer.concat([Buffer.from(t),d]);const c=Buffer.alloc(4);c.writeUInt32BE(crc(td));return Buffer.concat([l,td,c]);}
function encode(file,w,h,px){const raw=Buffer.alloc((w*4+1)*h);for(let y=0;y<h;y++){raw[y*(w*4+1)]=0;px.copy(raw,y*(w*4+1)+1,y*w*4,(y+1)*w*4);}const ih=Buffer.alloc(13);ih.writeUInt32BE(w,0);ih.writeUInt32BE(h,4);ih[8]=8;ih[9]=6;fs.writeFileSync(file,Buffer.concat([Buffer.from([137,80,78,71,13,10,26,10]),chunk('IHDR',ih),chunk('IDAT',zlib.deflateSync(raw)),chunk('IEND',Buffer.alloc(0))]));}
module.exports={decode,encode};
