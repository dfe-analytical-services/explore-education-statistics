import Gate from '@common/components/Gate';
import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';

describe('Gate', () => {
  test('renders children immediately if `condition` is true', () => {
    render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={true}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();
  });

  test('renders children immediately if `condition` function resolve to true', () => {
    render(
      <Gate condition={() => true}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();
  });

  test('does not render children immediately if `condition` is async', () => {
    render(
      <Gate
        condition={async () => {
          return true;
        }}
      >
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
  });

  test('renders children when async `condition` resolves to true', async () => {
    render(
      <Gate
        condition={async () => {
          return true;
        }}
      >
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(screen.getByText('children')).toBeInTheDocument();
    });
  });

  test('renders `loading` whilst async `condition` is resolving', async () => {
    render(
      <Gate
        condition={async () => {
          return true;
        }}
        loading={<p>loading</p>}
      >
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
    expect(screen.getByText('loading')).toBeInTheDocument();
  });

  test('does not render `loading` when async `condition` resolves to true', async () => {
    render(
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
      expect(screen.getByText('children')).toBeInTheDocument();
      expect(screen.queryByText('loading')).not.toBeInTheDocument();
    });
  });

  test('does not render `loading` when async `condition` resolves to false', async () => {
    render(
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
      expect(screen.queryByText('children')).not.toBeInTheDocument();
      expect(screen.queryByText('loading')).not.toBeInTheDocument();
    });
  });

  test('renders `fallback` when `condition` is false', () => {
    render(
      <Gate condition={false} fallback={<p>fallback</p>}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
    expect(screen.getByText('fallback')).toBeInTheDocument();
  });

  test('renders `fallback` when async `condition` resolves to false', async () => {
    render(
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
      expect(screen.queryByText('children')).not.toBeInTheDocument();
      expect(screen.getByText('fallback')).toBeInTheDocument();
    });
  });

  test('renders `fallback` with `condition` error', async () => {
    render(
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
      expect(screen.queryByText('children')).not.toBeInTheDocument();
      expect(screen.getByText('something went wrong')).toBeInTheDocument();
    });
  });

  test('does not unmount children if `condition` switches back to false', () => {
    const { rerender } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={true}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();

    rerender(
      <Gate condition={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();
  });

  test('does not unmount children if `condition` function resolves back to false', () => {
    const { rerender } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={() => true}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();

    rerender(
      <Gate condition={() => false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();
  });

  test('does not unmount children if async `condition` resolves back to false', async () => {
    const { rerender } = render(
      <Gate condition={() => Promise.resolve(true)}>
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(screen.getByText('children')).toBeInTheDocument();
    });

    rerender(
      <Gate condition={() => Promise.resolve(false)}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();
  });

  test('unmounts children if `condition` switches back to false and `passOnce = false`', () => {
    const { rerender } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={true} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();

    rerender(
      <Gate condition={false} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
  });

  test('unmounts children if `condition` function resolves back to false and `passOnce = false`', () => {
    const { rerender } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={() => true} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.getByText('children')).toBeInTheDocument();

    rerender(
      <Gate condition={() => false} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
  });

  test('unmounts children if async `condition` resolves back to false and `passOnce = false`', async () => {
    const { rerender } = render(
      // eslint-disable-next-line react/jsx-boolean-value
      <Gate condition={() => Promise.resolve(true)} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    await waitFor(() => {
      expect(screen.getByText('children')).toBeInTheDocument();
    });

    rerender(
      <Gate condition={() => Promise.resolve(false)} passOnce={false}>
        <p>children</p>
      </Gate>,
    );

    expect(screen.queryByText('children')).not.toBeInTheDocument();
  });
});
