import RouteLeavingGuard from '@admin/components/RouteLeavingGuard';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React from 'react';
import { Route, Router } from 'react-router';
import { Link } from 'react-router-dom';

describe('RouteLeavingGuard', () => {
  test('shows modal when route change is blocked on clicking link', () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <Route exact path="/">
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>

          <Link to="/other">Change route</Link>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    userEvent.click(screen.getByRole('link', { name: 'Change route' }));

    expect(history.location.pathname).toBe('/');

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByText('Test modal title')).toBeInTheDocument();
    expect(modal.getByText('Test modal content')).toBeInTheDocument();

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('shows modal when route change is blocked on `history.push`', () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <Route exact path="/">
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>

          <Link to="/other">Change route</Link>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    // We push an entry instead of clicking the link.
    // The result should still be the same.
    history.push('/other');

    expect(history.location.pathname).toBe('/');

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByText('Test modal title')).toBeInTheDocument();
    expect(modal.getByText('Test modal content')).toBeInTheDocument();

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('clicking confirm goes to the next route', () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <Route exact path="/">
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>

          <Link to="/other">Change route</Link>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    userEvent.click(screen.getByRole('link', { name: 'Change route' }));
    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(history.location.pathname).toBe('/other');
    expect(screen.getByText('Other route')).toBeInTheDocument();
  });

  test('clicking cancel does not change route', () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <Route exact path="/">
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>

          <Link to="/other">Change route</Link>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    userEvent.click(screen.getByRole('link', { name: 'Change route' }));
    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(history.location.pathname).toBe('/');

    expect(screen.getByText('Change route')).toBeInTheDocument();
    expect(screen.queryByText('Other route')).not.toBeInTheDocument();
  });

  test('does not show the modal when route change is not blocked', () => {
    const history = createMemoryHistory();

    render(
      <Router history={history}>
        <Route exact path="/">
          <Link to="/other">Change route</Link>

          <RouteLeavingGuard blockRouteChange={false} title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    userEvent.click(screen.getByRole('link', { name: 'Change route' }));

    expect(history.location.pathname).toBe('/other');

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();

    expect(screen.queryByText('Change route')).not.toBeInTheDocument();
    expect(screen.getByText('Other route')).toBeInTheDocument();
  });
});
