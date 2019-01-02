import React, { Component } from 'react';
import { BrowserRouter as Router, Route, Switch } from 'react-router-dom';
import './App.scss';
import Cookies from './pages/cookies'
import Privacy from './pages/privacy'
import Publication from './pages/publication';
import Theme from './pages/theme';
import Themes from './pages/themes';
import Topic from './pages/topic'
import Topics from './pages/topics'
import Feedback from './pages/feedback'
import NotFound from './pages/notfound'
import { Link } from 'react-router-dom'

class App extends Component {
  render() {
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
            <Route exact path="/themes/:theme/:topic/:publication" component={Publication} />

            
            <Route exact path="/topic/:topic" component={Topic} />
            <Route exact path="/theme/:theme" component={Theme} />
            <Route exact path="/topics" component={Topics} />
            <Route exact path="/publication/:publication" component={Publication} />

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
        <ul>
          <li><Link to={'/themes/'}>Themes</Link></li>
          <li><Link to={'/topics/'}>Topics</Link></li>
        </ul>
      </div>
    );
  }
}

export default App;
