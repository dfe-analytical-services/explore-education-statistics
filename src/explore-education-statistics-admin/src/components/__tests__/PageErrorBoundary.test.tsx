import PageErrorBoundary from '@admin/components/PageErrorBoundary';
import { useErrorControl } from '@common/contexts/ErrorControlContext';
import { render, waitFor } from '@testing-library/react';
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

    const { queryByText } = render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        queryByText('Sorry, there is a problem with the service'),
      ).not.toBeNull();
      expect(queryByText('Try again later.')).not.toBeNull();
    });
  });

  test('calling `handleError` with 401 error renders Forbidden page', async () => {
    const error: Partial<AxiosError> = {
      name: '',
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

    const { queryByText } = render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(queryByText('Forbidden')).not.toBeNull();
      expect(
        queryByText('You do not have permission to access this page.'),
      ).not.toBeNull();
    });
  });

  test('calling `handleError` with 404 error renders Not Found page', async () => {
    const error: Partial<AxiosError> = {
      name: '',
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

    const { queryByText } = render(
      <MemoryRouter>
        <PageErrorBoundary>
          <TestComponent />
        </PageErrorBoundary>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(queryByText('Resource not found')).not.toBeNull();
      expect(
        queryByText('There was a problem accessing a resource.'),
      ).not.toBeNull();
    });
  });
});
