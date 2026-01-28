declare module 'dotenv-json-complex' {
  export default function ({ environment: string });
}

declare module 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js' {
  export function htmlReport(data: unknown);
}
