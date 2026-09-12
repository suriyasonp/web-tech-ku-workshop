import { spawn } from 'node:child_process'
import { mkdir } from 'node:fs/promises'
import http from 'node:http'
import path from 'node:path'
import process from 'node:process'
import { chromium } from 'playwright'

const useMockApi = process.argv.includes('--mock-api')
const npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm'
const outputPath = path.resolve('docs/images/todo-app-demo.png')
const children = []
let mockServer

function startMockApi() {
  let nextId = 3
  const todos = [
    { id: 1, title: 'Complete the backend lab', isCompleted: true },
    { id: 2, title: 'Connect Vue to the API', isCompleted: false },
  ]

  mockServer = http.createServer((request, response) => {
    response.setHeader('Access-Control-Allow-Origin', 'http://127.0.0.1:5173')
    response.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization')
    response.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS')
    response.setHeader('Content-Type', 'application/json')
    if (request.method === 'OPTIONS') return response.end()

    let body = ''
    request.on('data', (chunk) => { body += chunk })
    request.on('end', () => {
      if (request.url === '/api/auth/login' && request.method === 'POST') {
        response.end(JSON.stringify({ accessToken: 'workshop-capture-token' }))
        return
      }
      if (request.url === '/api/todos' && request.method === 'GET') {
        response.end(JSON.stringify(todos))
        return
      }
      if (request.url === '/api/todos' && request.method === 'POST') {
        const todo = { id: nextId++, title: JSON.parse(body).title, isCompleted: false }
        todos.push(todo)
        response.statusCode = 201
        response.end(JSON.stringify(todo))
        return
      }
      const todoIdMatch = request.url?.match(/^\/api\/todos\/(\d+)$/)
      if (todoIdMatch && request.method === 'PUT') {
        const todo = todos.find((item) => item.id === Number(todoIdMatch[1]))
        if (!todo) {
          response.statusCode = 404
          response.end(JSON.stringify({ message: 'Not found' }))
          return
        }
        Object.assign(todo, JSON.parse(body))
        response.end(JSON.stringify(todo))
        return
      }
      response.statusCode = 404
      response.end(JSON.stringify({ message: 'Not found' }))
    })
  })
  mockServer.listen(5000, '127.0.0.1')
}

async function waitFor(url, timeoutMs = 60_000) {
  const startedAt = Date.now()
  while (Date.now() - startedAt < timeoutMs) {
    try {
      const response = await fetch(url)
      if (response.ok) return
    } catch {
      // The development server may still be starting.
    }
    await new Promise((resolve) => setTimeout(resolve, 500))
  }
  throw new Error(`Timed out waiting for ${url}`)
}

function stopProcesses() {
  for (const child of children) {
    if (!child.killed) child.kill('SIGTERM')
  }
  if (mockServer) mockServer.close()
}

try {
  if (useMockApi) {
    startMockApi()
  } else {
    children.push(spawn('dotnet', [
      'run',
      '--project',
      'backend/TodoApi/TodoApi.csproj',
      '--urls',
      'http://localhost:5000',
    ], { stdio: 'inherit' }))
  }

  children.push(spawn(npmCommand, [
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
  ], { stdio: 'inherit' }))

  await Promise.all([
    waitFor('http://localhost:5000'),
    waitFor('http://127.0.0.1:5173'),
  ])
  const customChromium = process.env.CHROMIUM_EXECUTABLE_PATH
  const browser = await chromium.launch({
    headless: true,
    ...(customChromium
      ? {
          executablePath: customChromium,
          args: ['--no-sandbox', '--disable-gpu', '--disable-software-rasterizer'],
        }
      : {}),
  })
  const page = await browser.newPage({ viewport: { width: 1440, height: 1000 }, deviceScaleFactor: 1 })

  await page.goto('http://127.0.0.1:5173', { waitUntil: 'networkidle' })
  await page.getByRole('button', { name: 'Sign in' }).click()
  await page.getByPlaceholder('What needs to be done?').fill('Capture the completed workshop app')
  await Promise.all([
    page.waitForResponse((response) =>
      response.url().endsWith('/api/todos') &&
      response.request().method() === 'POST' &&
      response.status() === 201),
    page.getByRole('button', { name: 'Add' }).click(),
  ])
  await page.getByRole('button', { name: 'Edit' }).last().click()
  await page.getByRole('textbox', { name: 'Edit Capture the completed workshop app' })
    .fill('Edit a Todo with the API')
  await Promise.all([
    page.waitForResponse((response) =>
      response.url().endsWith('/api/todos/3') &&
      response.request().method() === 'PUT' &&
      response.status() === 200),
    page.getByRole('button', { name: 'Save' }).click(),
  ])
  await page.getByText('Edit a Todo with the API').waitFor()

  await mkdir(path.dirname(outputPath), { recursive: true })
  await page.screenshot({ path: outputPath, fullPage: true })
  await browser.close()
  console.log(`Screenshot saved to ${outputPath}`)
} finally {
  stopProcesses()
}
