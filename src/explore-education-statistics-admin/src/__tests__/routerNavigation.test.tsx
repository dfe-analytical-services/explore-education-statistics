import { act, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import {
  createMemoryRouter,
  createRoutesStub,
  Link,
  MemoryRouter,
  Route,
  Routes,
} from 'react-router';
import { RouterProvider } from 'react-router/dom';
import TestLocationContext, {
  expectLocation,
} from '@admin/components/testing/TestLocationContext';

const RootPage = () => {
  return (
    <>
      <Link to="/other">Change route</Link>

      <TestLocationContext />
    </>
  );
};

const OtherPage = () => {
  return (
    <>
      <p>Other route</p>
      <TestLocationContext />
    </>
  );
};

const renderPageWithStub = () => {
  const Stub = createRoutesStub([
    {
      path: '/',
      Component: RootPage,
    },
    {
      path: '/other',
      Component: OtherPage,
    },
  ]);

  return {
    ...render(<Stub initialEntries={['/']} />),
  };
};

const renderPageWithRouter = () => {
  const memoryRouter = createMemoryRouter(
    [
      {
        path: '/',

        element: <RootPage />,
      },
      {
        path: '/other',
        element: <OtherPage />,
      },
    ],
    {
      initialEntries: ['/'],
    },
  );

  return {
    router: memoryRouter,

    ...render(<RouterProvider router={memoryRouter} />),
  };
};

const renderPageWithComponent = () => {
  return render(
    <MemoryRouter>
      <Routes>
        <Route path="/" element={<RootPage />} />
        <Route path="/other" element={<OtherPage />} />
      </Routes>
    </MemoryRouter>,
  );
};

describe('Router Navigation', () => {
  test('render with component', async () => {
    const user = userEvent.setup();

    renderPageWithComponent();

    await expectLocation('/');

    await user.click(screen.getByRole('link', { name: 'Change route' }));

    expect(await screen.findByText('Other route')).toBeInTheDocument();

    await expectLocation('/other');
  });

  test('render with stub', async () => {
    const user = userEvent.setup();

    renderPageWithStub();

    await expectLocation('/');

    await user.click(screen.getByRole('link', { name: 'Change route' }));

    expect(await screen.findByText('Other route')).toBeInTheDocument();

    await expectLocation('/other');
  });

  test('render with router', async () => {
    const user = userEvent.setup();

    renderPageWithRouter();

    await expectLocation('/');

    await user.click(screen.getByRole('link', { name: 'Change route' }));

    expect(await screen.findByText('Other route')).toBeInTheDocument();

    await expectLocation('/other');
  });

  // Direct manipulation of the router, this might uncover internal errors react router hides in
  // normal testing.
  test('router navigates directly', async () => {
    const { router } = renderPageWithRouter();

    expect(router.state.location.pathname).toBe('/');
    await act(async () => router.navigate('/other'));

    expect(router.state.location.pathname).toBe('/other');
    expect(await screen.findByText('Other route')).toBeInTheDocument();
  });
});
