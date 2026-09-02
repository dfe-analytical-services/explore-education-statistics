import RouteLeavingGuard from '@admin/components/RouteLeavingGuard';
import { act, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryRouter, RouterProvider } from 'react-router';
import { Link } from 'react-router-dom';
import TestLocationContext, {
  expectLocation,
} from '@admin/components/testing/TestLocationContext';

function renderPage(blockRouteChange = true) {
  const memoryRouter = createMemoryRouter(
    [
      {
        path: '/',
        element: (
          <>
            <RouteLeavingGuard
              blockRouteChange={blockRouteChange}
              title="Test modal title"
            >
              <p>Test modal content</p>
            </RouteLeavingGuard>

            <Link to="/other">Change route</Link>
            <TestLocationContext />
          </>
        ),
      },
      {
        path: '/other',
        element: (
          <>
            <p>Other route</p>
            <TestLocationContext />
          </>
        ),
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
}

describe('RouteLeavingGuard', () => {
  test('shows modal when route change is blocked on clicking link', async () => {
    renderPage();

    await userEvent.click(screen.getByRole('link', { name: 'Change route' }));

    await expectLocation('/');

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByText('Test modal title')).toBeInTheDocument();
    expect(modal.getByText('Test modal content')).toBeInTheDocument();

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('shows modal when route change is blocked when location is externally changed', async () => {
    const { router } = renderPage();

    act(() => {
      // externally change the url
      // this is as close as we can get to changing history
      router.navigate('/other');
    });

    await expectLocation('/');

    expect(await screen.findByText('Test modal title')).toBeInTheDocument();

    expect(
      within(screen.getByRole('dialog')).getByText('Test modal content'),
    ).toBeInTheDocument();

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('clicking confirm goes to the next route', async () => {
    renderPage();

    await userEvent.click(screen.getByRole('link', { name: 'Change route' }));
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(async () => expectLocation('/other'));

    expect(screen.getByText('Other route')).toBeInTheDocument();
  });

  test('clicking cancel does not change route', async () => {
    renderPage();

    await userEvent.click(screen.getByRole('link', { name: 'Change route' }));
    await userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    await expectLocation('/');

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('does not show the modal when route change is not blocked', async () => {
    renderPage(false);

    await userEvent.click(screen.getByRole('link', { name: 'Change route' }));

    await expectLocation('/other');

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

    expect(screen.queryByText('Change route')).not.toBeInTheDocument();
    expect(screen.getByText('Other route')).toBeInTheDocument();
  });
});
