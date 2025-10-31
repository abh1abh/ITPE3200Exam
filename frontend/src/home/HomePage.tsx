import React from "react";
import { Carousel, Image } from "react-bootstrap";
const API_URL = import.meta.env.VITE_API_URL;
console.log(API_URL);

const HomePage: React.FC = () => {
  return (
    <div className="text-center">
      <h1 className="display-4">Homecare Appointment Management</h1>
      <Carousel>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/man_happy.jpg`}
            className="d-block w-100"
            alt="Happy man"
            style={{ height: "500px", objectFit: "cover", objectPosition: "center" }}
          />
        </Carousel.Item>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/man_with_nurse.jpg`}
            className="d-block w-100"
            alt="Man with nurse"
            style={{ height: "500px", objectFit: "cover", objectPosition: "center" }}
          />
        </Carousel.Item>
        <Carousel.Item>
          <Image
            src={`${API_URL}/images/doctor.jpg`}
            className="d-block w-100"
            alt="Doctor"
            style={{ height: "500px", objectFit: "cover", objectPosition: "center" }}
          />
        </Carousel.Item>
      </Carousel>
    </div>
  );
};

export default HomePage;
