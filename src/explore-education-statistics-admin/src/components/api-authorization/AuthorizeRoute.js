/* eslint-disable */
import React from 'react';
import { Component } from 'react';
import { Route, Redirect } from 'react-router-dom';
import { LoginContext } from '../Login';
import {
  ApplicationPaths,
  QueryParameterNames,
} from './ApiAuthorizationConstants';
import authService from './AuthorizeService';
import loginService from '@admin/services/sign-in/service';

export default class AuthorizeRoute extends Component {
  constructor(props) {
    super(props);

    this.state = {
      ready: false,
      // HIVE
      // swapped "authenticated" for "user" here to support adding user profile in a Context for components
      // further down in the component hierarchy
      user: undefined,
    };
  }

  componentDidMount() {
    this._subscription = authService.subscribe(() =>
      this.authenticationChanged(),
    );
    this.populateAuthenticationState();
  }

  componentWillUnmount() {
    authService.unsubscribe(this._subscription);
  }

  render() {
    const { ready, user } = this.state;
    const redirectUrl = `${ApplicationPaths.Login}?${
      QueryParameterNames.ReturnUrl
    }=${encodeURI(window.location.href)}`;
    if (!ready) {
      return <div></div>;
    } else {
      const {
        component: Component,
        renderIfNotAuthenticated,
        ...rest
      } = this.props;
      return (
        <Route
          {...rest}
          render={props => {
            if (user || renderIfNotAuthenticated) {
              return (
                // HIVE
                // added LoginContext tags here to surround Component to allow users to be available as a
                // Context further down in the component hierarchy
                <>
                  <LoginContext.Provider value={{ user }}>
                    <Component {...props} />
                  </LoginContext.Provider>
                </>
              );
            } else {
              return <Redirect to={redirectUrl} />;
            }
          }}
        />
      );
    }
  }

  async populateAuthenticationState() {
    const authenticated = await authService.isAuthenticated();

    if (authenticated) {
      // HIVE
      // calling our own service to retrieve user details from our own User domain model
      await loginService
        .getUserDetails()
        .then(user => this.setState({ ready: true, user }))
        .catch(() => this.setState({ ready: false, user: undefined }));
    } else {
      this.setState({ ready: true, user: undefined });
    }
  }

  async authenticationChanged() {
    this.setState({ ready: false, user: undefined });
    await this.populateAuthenticationState();
  }
}
/* eslint-enable */
