import React from 'react';

class ScrollToTop extends React.Component<{}> {
  public componentDidUpdate() {
    window.scrollTo(0, 0);
  }
  public render() {
    return this.props.children;
  }
}

export default ScrollToTop;
