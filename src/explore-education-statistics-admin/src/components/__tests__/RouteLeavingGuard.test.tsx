import RouteLeavingGuard from '@admin/components/RouteLeavingGuard';
import { fireEvent, render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { createMemoryHistory } from 'history';
import React from 'react';
import { MemoryRouter, Route, Router } from 'react-router';
import { Link } from 'react-router-dom';

describe('RouteLeavingGuard', () => {
  test('shows the modal when route change is blocked', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <Route exact path="/">
          <Link to="/other">Change route</Link>
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </MemoryRouter>,
    );

    fireEvent.click(screen.getByText('Change route'));

    const modal = within(screen.getByRole('dialog'));
    expect(modal.getByText('Test modal title')).toBeInTheDocument();
    expect(modal.getByText('Test modal content')).toBeInTheDocument();
  });

  test('clicking confirm goes to the next route', () => {
    const history = createMemoryHistory();
    render(
      <Router history={history}>
        <Route exact path="/">
          <Link to="/other">Change route</Link>
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    fireEvent.click(screen.getByText('Change route'));

    userEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(history.location.pathname).toEqual('/other');
  });

  test('clicking cancel does not change route', () => {
    const history = createMemoryHistory();
    render(
      <Router history={history}>
        <Route exact path="/">
          <Link to="/other">Change route</Link>
          <RouteLeavingGuard blockRouteChange title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </Router>,
    );

    fireEvent.click(screen.getByText('Change route'));

    userEvent.click(screen.getByRole('button', { name: 'Cancel' }));

    expect(history.location.pathname).toEqual('/');
  });

  test('does not show the modal when route change is not blocked', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <Route exact path="/">
          <Link to="/other">Change route</Link>
          <RouteLeavingGuard blockRouteChange={false} title="Test modal title">
            <p>Test modal content</p>
          </RouteLeavingGuard>
        </Route>
        <Route exact path="/other">
          <p>Other route</p>
        </Route>
      </MemoryRouter>,
    );

    fireEvent.click(screen.getByText('Change route'));

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
  });
});
