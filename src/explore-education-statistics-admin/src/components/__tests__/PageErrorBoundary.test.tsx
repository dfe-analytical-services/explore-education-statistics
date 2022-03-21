import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { render, screen, waitFor } from '@testing-library/react';
import { AxiosError } from 'axios';
import React, { useEffect } from 'react';
import { MemoryRouter } from 'react-router';

describe('PageErrorBoundary', () => {
  test('calling `handleError` renders generic error page', async () => {
    const error = new Error('Something went wrong');

    const TestComponent = () => {
      const { handleError } = useErrorControl();

      useEffect(() => {
        Promise.reject(error).catch(err => {
          handleError(err);
        });
      }, [handleError]);

      return null;
    };

    render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText('Sorry, there is a problem with the service'),
      ).toBeInTheDocument();
      expect(screen.getByText('Try again later.')).toBeInTheDocument();
    });
  });

  test('calling `handleError` with 401 error renders Forbidden page', async () => {
    const error: Partial<AxiosError> = {
      name: 'Error',
      message: 'Forbidden',
      isAxiosError: true,
      request: {},
      response: {
        config: {},
        data: {},
        headers: {},
        status: 401,
        statusText: 'Forbidden',
        request: {},
      },
    };

    const TestComponent = () => {
      const { handleError } = useErrorControl();

      useEffect(() => {
        Promise.reject(error).catch(err => {
          handleError(err);
        });
      }, [handleError]);

      return null;
    };

    render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Forbidden')).toBeInTheDocument();
      expect(
        screen.getByText('You do not have permission to access this page.'),
      ).toBeInTheDocument();
    });
  });

  test('calling `handleError` with 404 error renders Not Found page', async () => {
    const error: Partial<AxiosError> = {
      name: 'Error',
      message: 'Not Found',
      isAxiosError: true,
      request: {},
      response: {
        config: {},
        data: {},
        headers: {},
        status: 404,
        statusText: 'Not Found',
        request: {},
      },
    };

    const TestComponent = () => {
      const { handleError } = useErrorControl();

      useEffect(() => {
        Promise.reject(error).catch(err => {
          handleError(err);
        });
      }, [handleError]);

      return null;
    };

    render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Resource not found')).toBeInTheDocument();
      expect(
        screen.getByText('There was a problem accessing a resource.'),
      ).toBeInTheDocument();
    });
  });
});
