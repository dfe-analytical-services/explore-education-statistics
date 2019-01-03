import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import Cookies from './pages/Cookies';
import Feedback from './pages/Feedback';
import Home from './pages/Home';
import NotFound from './pages/NotFound';
import Privacy from './pages/Privacy';
import PublicationPage from './pages/PublicationPage';
import Publications from './pages/Publications';
import Theme from './pages/Theme';
import Themes from './pages/Themes';
import Topic from './pages/Topic';
import Topics from './pages/Topics';

import './App.scss';

class App extends Component {
  public render() {
    return (
      <div className="App">
        <Router>
          <Switch>
            <Route exact path="/" component={Home} />
            <Route exact path="/cookies" component={Cookies} />
            <Route exact path="/privacy-policy" component={Privacy} />
            <Route exact path="/feedback" component={Feedback} />

            <Route exact path="/themes" component={Themes} />
            <Route exact path="/themes/:theme" component={Theme} />
            <Route exact path="/themes/:theme/:topic" component={Topic} />
            <Route
              exact
              path="/themes/:theme/:topic/:publication"
              component={PublicationPage}
            />

            <Route exact path="/topics" component={Topics} />
            <Route exact path="/topics/:topic" component={Topic} />
            <Route
              exact
              path="/topics/:topic/:publication"
              component={PublicationPage}
            />

            <Route exact path="/publications/" component={Publications} />
            <Route
              exact
              path="/publications/:publication"
              component={PublicationPage}
            />

            <Route component={NotFound} />
          </Switch>
        </Router>
      </div>
    );
  }
}

export default App;
