import UserTestingBanner from '@frontend/components/UserTestingBanner';
import * as _useCookies from '@frontend/hooks/useCookies';
import React from 'react';
import { screen, render } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

jest.mock('@frontend/hooks/useCookies');
const useCookies = _useCookies as jest.Mocked<typeof _useCookies>;

describe('UserTestingBanner', () => {
  test('renders the banner if the cookie is not set', () => {
    jest.spyOn(useCookies, 'useCookies').mockImplementation(() => ({
      getCookie: () => {
        return undefined;
      },
      setBannerSeenCookie: jest.fn(),
      setUserTestingBannerSeenCookie: jest.fn(),
      setGADisabledCookie: jest.fn(),
    }));

    render(<UserTestingBanner />);

    expect(
      screen.getByText('Shape the future of Explore education statistics'),
    ).toBeInTheDocument();
  });

  test('renders the banner if the cookie value does not have the version', () => {
    jest.spyOn(useCookies, 'useCookies').mockImplementation(() => ({
      getCookie: () => {
        return 'true';
      },
      setBannerSeenCookie: jest.fn(),
      setUserTestingBannerSeenCookie: jest.fn(),
      setGADisabledCookie: jest.fn(),
    }));

    render(<UserTestingBanner />);

    expect(
      screen.getByText('Shape the future of Explore education statistics'),
    ).toBeInTheDocument();
  });

  test('renders the banner if the cookie value does not match the current version', () => {
    jest.spyOn(useCookies, 'useCookies').mockImplementation(() => ({
      getCookie: () => {
        return JSON.stringify({ version: 999 });
      },
      setBannerSeenCookie: jest.fn(),
      setUserTestingBannerSeenCookie: jest.fn(),
      setGADisabledCookie: jest.fn(),
    }));

    render(<UserTestingBanner />);

    expect(
      screen.getByText('Shape the future of Explore education statistics'),
    ).toBeInTheDocument();
  });

  test('clicking the close button sets the cookie', async () => {
    const handleUserTestingBannerSeen = jest.fn();
    jest.spyOn(useCookies, 'useCookies').mockImplementation(() => ({
      getCookie: () => {
        return undefined;
      },
      setBannerSeenCookie: jest.fn(),
      setUserTestingBannerSeenCookie: handleUserTestingBannerSeen,
      setGADisabledCookie: jest.fn(),
    }));

    render(<UserTestingBanner />);

    expect(handleUserTestingBannerSeen).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole('button', { name: /Close/ }));

    expect(handleUserTestingBannerSeen).toHaveBeenCalledWith(
      useCookies.userTestingBannerVersion,
    );
  });

  test('does not render the banner if the cookie value matches the current version', () => {
    jest.spyOn(useCookies, 'useCookies').mockImplementation(() => ({
      getCookie: () => {
        return JSON.stringify({ version: useCookies.userTestingBannerVersion });
      },
      setBannerSeenCookie: jest.fn(),
      setUserTestingBannerSeenCookie: jest.fn(),
      setGADisabledCookie: jest.fn(),
    }));

    render(<UserTestingBanner />);

    expect(
      screen.queryByText('Shape the future of Explore education statistics'),
    ).not.toBeInTheDocument();
  });
});
