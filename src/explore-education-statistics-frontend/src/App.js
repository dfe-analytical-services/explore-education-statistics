import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import './App.scss';
import Cookies from './pages/cookies'
import Privacy from './pages/privacy'
import NotFound from './pages/notfound'

class App extends Component {
  render() {
    return (
      <div className="App">
        <Router>
          <Switch>
            <Route exact path="/" component={Home} />
            <Route exact path="/cookies" component={Cookies} />
            <Route exact path="/privacy-policy" component={Privacy} />
            <Route component={NotFound} />
          </Switch>
        </Router>
      </div>
    );
  }
}

class Home extends Component {
  render() {
    return (
      <div>
        <h1 className="govuk-heading-xl">Explore education statistics</h1>
      </div>
    );
  }
}

export default App;
