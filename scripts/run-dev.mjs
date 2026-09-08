import { spawn } from 'node:child_process'
import process from 'node:process'

const npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm'

const children = [
  spawn('dotnet', [
    'run',
    '--project',
    'backend/TodoApi/TodoApi.csproj',
    '--urls',
    'http://localhost:5000',
  ], { stdio: 'inherit' }),
  spawn(npmCommand, [
    'run',
    'dev',
    '--prefix',
    'frontend',
    '--',
    '--host',
    '127.0.0.1',
    '--port',
    '5173',
    '--strictPort',
  ], { stdio: 'inherit' }),
]

let stopping = false

function stop(exitCode = 0) {
  if (stopping) return
  stopping = true
  for (const child of children) {
    if (!child.killed) child.kill('SIGTERM')
  }
  setTimeout(() => process.exit(exitCode), 500)
}

for (const child of children) {
  child.on('error', (error) => {
    console.error(error.message)
    stop(1)
  })
  child.on('exit', (code) => {
    if (!stopping && code !== 0) stop(code ?? 1)
  })
}

process.on('SIGINT', () => stop())
process.on('SIGTERM', () => stop())

console.log('Todo API:      http://localhost:5000')
console.log('API Reference: http://localhost:5000/scalar/v1')
console.log('Vue frontend:  http://localhost:5173')
