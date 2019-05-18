//
//  ViewController.swift
//  parkapp
//
//  Created by Ilya Sedov on 19/05/2019.
//  Copyright © 2019 SKB Kontur. All rights reserved.
//

import UIKit

class ViewController: UIViewController {
  @IBOutlet weak var vacantMarker: UIView!
  @IBOutlet weak var occupiedMarker: UIView!
  @IBOutlet weak var vacantLabel: UILabel!
  @IBOutlet weak var occupiedLabel: UILabel!
  @IBOutlet weak var totalLabel: UILabel!

  var parking: Parking!

  override func viewDidLoad() {
    super.viewDidLoad()
    vacantMarker.layer.cornerRadius = vacantMarker.bounds.height/2.0
    occupiedMarker.layer.cornerRadius = occupiedMarker.bounds.height/2.0

    title = parking.title
    vacantLabel.text = String(format: NSLocalizedString("places_vacant", comment: ""), parking.vacant)
    occupiedLabel.text = String(format: NSLocalizedString("places_occupied", comment: ""), parking.occupied)
    totalLabel.text = String(format: NSLocalizedString("places_total", comment: ""), parking.total)
  }

  @IBAction func notifyVacant(_ sender: Any) {
  }

  override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
    if segue.identifier == "showCam" {
      guard let camVc = segue.destination as? CamViewController else { return }
      camVc.camId = parking.number
    }
  }
}

