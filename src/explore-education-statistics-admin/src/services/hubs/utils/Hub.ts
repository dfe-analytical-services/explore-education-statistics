/* eslint-disable @typescript-eslint/no-explicit-any */
import isHubResult from '@admin/services/hubs/utils/isHubResult';
import logger from '@common/services/logger';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

export interface Subscription {
  methodName: string;
  callback(...args: any[]): void;
  unsubscribe(): void;
}

export default class Hub {
  constructor(protected connection: HubConnection) {}

  status(): HubConnectionState {
    return this.connection.state;
  }

  async start(): Promise<void> {
    if (this.connection.state !== 'Connected') {
      await this.connection.start();
    }
  }

  async stop(): Promise<void> {
    if (this.connection.state !== 'Connected') {
      await this.connection.stop();
    }
  }

  async invoke<T>(methodName: string, ...args: unknown[]): Promise<T> {
    logger.debug(`Invoking hub '${methodName}'`, {
      url: this.connection.baseUrl,
      args,
    });

    const result = await this.connection.invoke(methodName, ...args);

    logger.debug(`Invoking hub '${methodName}' succeeded`, {
      url: this.connection.baseUrl,
      result,
    });

    // Automatically try and unwrap hub results
    // if they contain data (for convenience).
    if (isHubResult<T>(result) && result.data) {
      return result.data;
    }

    return result as T;
  }

  async send(methodName: string, ...args: unknown[]): Promise<void> {
    if (this.connection.state !== 'Connected') {
      return;
    }

    logger.debug(`Sending to hub '${methodName}`, {
      url: this.connection.baseUrl,
      args,
    });

    await this.connection.send(methodName, ...args);
  }

  subscribe(
    methodName: string,
    callback: (...args: any[]) => void,
  ): Subscription {
    this.connection.on(methodName, callback);

    return {
      methodName,
      callback,
      unsubscribe: () => this.connection.off(methodName, callback),
    };
  }

  onDisconnect(callback: (error?: Error) => void): void {
    this.connection.onclose(callback);
  }

  onReconnected(callback: (connectionId?: string) => void): void {
    this.connection.onreconnected(callback);
  }

  onReconnecting(callback: (error?: Error) => void): void {
    this.connection.onreconnecting(callback);
  }
}
