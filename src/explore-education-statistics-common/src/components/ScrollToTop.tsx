import { SingletonRouter, withRouter } from 'next/router';
import React, { Component } from 'react';

interface Props {
  router: SingletonRouter;
}

class ScrollToTop extends Component<Props> {
  public componentDidUpdate(prevProps: Props) {
    if (!this.props.router || !prevProps.router) {
      return;
    }

    if (this.props.router.route !== prevProps.router.route) {
      window.scrollTo(0, 0);
    }
  }

  public render() {
    return <>this.props.children</>;
  }
}

export default withRouter(ScrollToTop);
