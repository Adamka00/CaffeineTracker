const { test } = require('node:test');
const assert = require('node:assert/strict');
const state = require('../../Caffeine/wwwroot/js/experience-state.js');
test('release dismissal suppresses same version only',()=>{
 assert.equal(state.releaseDue('4.0','4.0'),false);assert.equal(state.releaseDue('3.0','4.0'),true);
});
test('install needs three distinct visits, support, and no standalone',()=>{
 let s={};s=state.visit(s,'2026-10-01');s=state.visit(s,'2026-10-01');assert.equal(s.visits,1);
 s=state.visit(s,'2026-10-02');s=state.visit(s,'2026-10-03');
 assert.equal(state.installDue(s,0,false,true),true);assert.equal(state.installDue(s,0,true,true),false);assert.equal(state.installDue(s,0,false,false),false);
});
test('later lasts seven days and never preference persists',()=>{
 const s=state.later({visits:3},1000);assert.equal(state.installDue(s,1001,false,true),false);assert.equal(state.installDue(s,1000+7*86400000,false,true),true);
 assert.equal(state.installDue(state.never(s),1e12,false,true),false);
});
test('iPhone and desktop-UA iPad get honest manual install instructions',()=>{
 assert.equal(state.platform('iPhone',1,'iPhone'),'ios');assert.equal(state.platform('Mozilla',5,'MacIntel'),'ios');assert.equal(state.platform('Mozilla',0,'MacIntel'),'other');
});
