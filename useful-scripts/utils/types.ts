import { ExecaChildProcess, StdoutStderrAll } from 'execa';
import { ChildProcessWithoutNullStreams } from 'node:child_process';

export type ExecaChildProcessWithoutNullStreams<
  StdoutStderrType extends StdoutStderrAll = string,
> = ExecaChildProcess<StdoutStderrType> & ChildProcessWithoutNullStreams;
