import Gate from '@common/components/Gate';
import { render, waitFor } from '@testing-library/react';
import React from 'react';

describe('Gate', () => {
  test('renders children immediately if `condition` is true', () => {
    const { queryByText } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={true}>
        <p>children</p>
      </Gate>,
    );

    expect(queryByText('children')).toBeInTheDocument();
  });

  test('does not render children immediately if `condition` is async', () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return true;
        }}
      >
        <p>children</p>
      </Gate>,
    );

    expect(queryByText('children')).not.toBeInTheDocument();
  });

  test('renders children when async `condition` resolves to true', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return true;
        }}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(queryByText('children')).toBeInTheDocument();
    });
  });

  test('renders `loading` whilst async `condition` is resolving', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return true;
        }}
        loading={<p>loading</p>}
      >
        <p>children</p>
      </Gate>,
    );

    expect(queryByText('children')).not.toBeInTheDocument();
    expect(queryByText('loading')).toBeInTheDocument();
  });

  test('does not render `loading` when async `condition` resolves to true', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return true;
        }}
        loading={<p>loading</p>}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(queryByText('children')).toBeInTheDocument();
      expect(queryByText('loading')).not.toBeInTheDocument();
    });
  });

  test('does not render `loading` when async `condition` resolves to false', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return false;
        }}
        loading={<p>loading</p>}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(queryByText('children')).not.toBeInTheDocument();
      expect(queryByText('loading')).not.toBeInTheDocument();
    });
  });

  test('renders `fallback` when `condition` is false', () => {
    const { queryByText } = render(
      <Gate condition={false} fallback={<p>fallback</p>}>
        <p>children</p>
      </Gate>,
    );

    expect(queryByText('children')).not.toBeInTheDocument();
    expect(queryByText('fallback')).toBeInTheDocument();
  });

  test('renders `fallback` when async `condition` resolves to false', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          return false;
        }}
        fallback={<p>fallback</p>}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(queryByText('children')).not.toBeInTheDocument();
      expect(queryByText('fallback')).toBeInTheDocument();
    });
  });

  test('renders `fallback` with `condition` error', async () => {
    const { queryByText } = render(
      <Gate
        condition={async () => {
          throw new Error('something went wrong');
        }}
        fallback={(error: Error) => <p>{error?.message}</p>}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(queryByText('children')).not.toBeInTheDocument();
      expect(queryByText('something went wrong')).toBeInTheDocument();
    });
  });
});
