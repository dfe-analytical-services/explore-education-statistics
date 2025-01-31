import client from '@admin/services/utils/service';

export interface GlobalPermissions {
  canAccessSystem: boolean;
  canAccessPrereleasePages: boolean;
  canAccessAnalystPages: boolean;
  canAccessAllImports: boolean;
  canManageAllTaxonomy: boolean;
  isBauUser: boolean;
  isApprover: boolean;
}

export interface UserProfile {
  id: string;
  firstName: string;
}

/**
 * This interface represents the response from our local login / registration
 * endpoint, which is used to verify that a user who has accessed the service
 * successfully from the Identity Provider actually has roles and permissions
 * to use the service. This endpoint will determine if the user is either:
 *
 * 1) An existing user on the service (resulting in a "LoginSuccess" and
 * details of the user's local profile and permissions).
 *
 * 2) A new user to the service with a valid invite to the service (resulting in
 * a "RegistrationSuccess" and details of the user's local profile and
 * permissions).
 *
 * 3) A user who has signed in successfully via the Identity Provider but who
 * has no invitation to use the service (resulting in a "NoInvite" and no
 * user profile or permissions).
 *
 * 4) A user who has signed in successfully via the Identity Provider but who
 * has an expired invitation to use the service (resulting in an "ExpiredInvite"
 * and no user profile or permissions).
 */

export type SignInResponse =
  | {
      loginResult: 'LoginSuccess' | 'RegistrationSuccess';
      userProfile: UserProfile;
    }
  | {
      loginResult: 'NoInvite' | 'ExpiredInvite';
    };

const authService = {
  signIn(): Promise<SignInResponse> {
    return client.post('/sign-in');
  },
};

export default authService;
