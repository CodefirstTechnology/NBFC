#!/usr/bin/env node
/** Quick check that shell + all MFE dev servers are listening. */
const ports = [
  { name: 'shell', port: 4200 },
  { name: 'mfe-member', port: 4201 },
  { name: 'mfe-deposit', port: 4202 },
  { name: 'mfe-loan', port: 4203 },
  { name: 'mfe-collection', port: 4204 },
  { name: 'mfe-recovery', port: 4205 },
  { name: 'mfe-accounting', port: 4206 },
  { name: 'mfe-reports', port: 4207 },
  { name: 'mfe-admin', port: 4208 },
];

async function probe(port) {
  try {
    const res = await fetch(`http://localhost:${port}/`, { signal: AbortSignal.timeout(3000) });
    return res.ok || res.status < 500;
  } catch {
    return false;
  }
}

let up = 0;
for (const { name, port } of ports) {
  const ok = await probe(port);
  console.log(`${ok ? 'OK  ' : 'DOWN'}  ${name.padEnd(16)} http://localhost:${port}`);
  if (ok) up++;
}

console.log(`\n${up}/${ports.length} frontend apps responding.`);
if (up < ports.length) {
  console.log('\nIf using Docker frontend, stop it first:');
  console.log('  docker compose stop shell mfe-member mfe-deposit mfe-loan mfe-collection mfe-recovery mfe-accounting mfe-reports mfe-admin');
  console.log('\nThen restart local dev: npm run start:all');
  process.exit(1);
}
