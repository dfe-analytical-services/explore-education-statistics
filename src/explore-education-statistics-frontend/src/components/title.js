import React, { Component } from 'react';
import PropTypes from 'prop-types'

class Title extends Component {
    render() {
        return (
            <h1 className="govuk-heading-xl">{this.props.label}</h1>
        );
    }
}

Title.propTypes = {
    label: PropTypes.string
}

Title.defaultProps = {
    label: ''
}

export default Title;