import React, { Component } from 'react';
import { Link } from 'react-router-dom'

class Glink extends Component {
    render() {
        return (
            <Link to="#" className="govuk-link govuk-link--no-visited-state">{this.props.children}</Link>
        );
    }
}

export default Glink;