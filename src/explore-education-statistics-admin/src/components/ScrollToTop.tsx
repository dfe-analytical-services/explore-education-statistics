import React, { Component } from 'react';
import { RouteComponentProps, withRouter } from 'react-router';

class ScrollToTop extends Component<RouteComponentProps> {
  public componentDidUpdate(prevProps: RouteComponentProps) {
    const { location } = this.props;

    if (location.pathname !== prevProps.location.pathname) {
      window.scrollTo(0, 0);
    }
  }

  public render() {
    return <>this.props.children</>;
  }
}

export default withRouter(ScrollToTop);
