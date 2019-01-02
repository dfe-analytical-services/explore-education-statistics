import React, { Component } from 'react';
import axios from 'axios';
import Title from '../components/title';

class Publication extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        axios.get(`http://localhost:5010/api/Publication/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label={data.title} />
                </div>
            </div>
        );
    }
}

export default Publication;