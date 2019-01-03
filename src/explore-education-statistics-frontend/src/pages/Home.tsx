import React, { Component } from 'react';
import { Link } from 'react-router-dom';

class Home extends Component {
  render() {
    return (
      <div>
        <h1 className="govuk-heading-xl">Explore education statistics</h1>
        <ul className="govuk-list">
          <li><Link to={'/themes'} className="govuk-link">Themes</Link></li>
          <li><Link to={'/topics'} className="govuk-link">Topics</Link></li>
          <li><Link to={'/publications'} className="govuk-link">Publications</Link></li>

        </ul>
      </div>
    );
  }
}

export default Home;
